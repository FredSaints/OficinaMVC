using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OficinaMVC.Data.Entities;
using OficinaMVC.Helpers;
using OficinaMVC.Models.Accounts;
using OficinaMVC.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace OficinaMVC.Controllers
{
    /// <summary>
    /// Handles user account-related actions such as login, registration, password management, and profile updates.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _configuration;
        private readonly IImageHelper _imageHelper;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userHelper">Helper for user management operations.</param>
        /// <param name="mailHelper">Helper for sending emails.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="imageHelper">Helper for image upload and processing.</param>
        /// <param name="signInManager">ASP.NET Core Identity sign-in manager.</param>
        /// <param name="userService">Service for user-related business logic.</param>
        public AccountController(IUserHelper userHelper,
                                 IMailHelper mailHelper,
                                 IConfiguration configuration,
                                 IImageHelper imageHelper,
                                 SignInManager<User> signInManager,
                                 IUserService userService)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _configuration = configuration;
            _imageHelper = imageHelper;
            _signInManager = signInManager;
            _userService = userService;
        }

        /// <summary>
        /// Displays the login page.
        /// </summary>
        /// <returns>The login view or redirects if already authenticated.</returns>
        [HttpGet]
        // GET: Account/Login
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }


        /// <summary>
        /// Handles user login POST requests.
        /// </summary>
        /// <param name="model">The login view model containing user credentials.</param>
        /// <returns>Redirects on success or returns the login view on failure.</returns>
        [HttpPost]
        // POST: Account/Login
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),

                new Claim("FullName", user.FullName),
                new Claim("ProfileImageUrl", user.ProfileImageUrl ?? "/images/default-profile.png")
            };

                    var userRoles = await _userHelper.GetRolesAsync(user);

                    foreach (var role in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    await _userHelper.SignInWithClaimsAsync(user, model.RememberMe, claims);

                    if (userRoles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    if (userRoles.Contains("Receptionist") || userRoles.Contains("Mechanic"))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>Redirects to the home page.</returns>
        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Displays the user registration page. Only accessible by Admins.
        /// </summary>
        /// <returns>The registration view.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        // GET: Account/Register
        public IActionResult Register()
        {
            ViewBag.Roles = RolesHelper.Roles;
            return View();
        }

        /// <summary>
        /// Handles user registration POST requests. Only accessible by Admins.
        /// </summary>
        /// <param name="model">The registration view model.</param>
        /// <returns>The registration confirmation view or the registration view with errors.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        // POST: Account/Register
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = RolesHelper.Roles;
                return View(model);
            }


            var existingUser = await _userHelper.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email already in use.");
                ViewBag.Roles = RolesHelper.Roles;
                return View(model);
            }

            var existingNif = await _userHelper.GetUserByNifAsync(model.NIF);
            if (existingNif != null)
            {
                ModelState.AddModelError(nameof(model.NIF), "NIF already in use.");
                ViewBag.Roles = RolesHelper.Roles;
                return View(model);
            }

            string? imageUrl = null;
            if (model.ProfileImage != null)
                imageUrl = await _imageHelper.UploadImageAsync(model.ProfileImage, "users");

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email,
                NIF = model.NIF,
                PhoneNumber = model.PhoneNumber,
                ProfileImageUrl = imageUrl,
                EmailConfirmed = false
            };

            var tempPassword = GenerateRandomPassword();

            var result = await _userHelper.AddUserAsync(user, tempPassword);

            if (result.Succeeded)
            {
                await _userHelper.CheckRoleAsync(model.Role);
                await _userHelper.AddUserToRoleAsync(user, model.Role);

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = UrlEncoder.Default.Encode(token);
                var link = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, token = encodedToken },
                    protocol: HttpContext.Request.Scheme);

                var response = _mailHelper.SendEmail(
                    user.Email,
                    "Welcome to FredAuto - Your Account is Ready",
                    $"<h1>Welcome to FredAuto!</h1>" +
                    $"<p>An account has been created for you by an administrator.</p>" +
                    $"<p>You can log in immediately using the following temporary password:</p>" +
                    $"<h3 style='font-family: monospace; background-color: #f0f0f0; padding: 10px; border-radius: 5px;'>{tempPassword}</h3>" +
                    $"<p>Before you begin, please confirm your email address by clicking the link below:</p>" +
                    $"<p><a href='{link}' style='padding: 10px 15px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>Confirm My Email Address</a></p>" +
                    $"<p>For your security, we strongly recommend you change your password after your first login.</p>"
                );

                ViewBag.Message = response.IsSuccess
                    ? "User created successfully! The user has been sent their temporary password and an email confirmation link."
                    : "User created, but the welcome email could not be sent.";

                if (model.Role == "Mechanic")
                {
                    TempData["SuccessMessage"] = "Mechanic created! Now configure their specialties and schedule.";
                    return RedirectToAction("Edit", "Mechanics", new { id = user.Id });
                }

                return View("RegisterConfirmation");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Roles = RolesHelper.Roles;
            return View(model);
        }

        /// <summary>
        /// Displays the form to change the current user's profile information.
        /// </summary>
        /// <returns>The change user view.</returns>
        [HttpGet]
        // GET: Account/ChangeUser
        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var model = new ChangeUserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                CurrentProfileImageUrl = user.ProfileImageUrl
            };

            return View(model);
        }

        /// <summary>
        /// Handles profile update POST requests for the current user.
        /// </summary>
        /// <param name="model">The change user view model.</param>
        /// <returns>The updated profile view or the view with errors.</returns>
        [HttpPost]
        // POST: Account/ChangeUser
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            string oldImageUrl = user.ProfileImageUrl;
            if (model.ProfileImage != null)
            {
                user.ProfileImageUrl = await _imageHelper.UploadImageAsync(model.ProfileImage, "users");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userHelper.UpdateUserAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                var updatedModel = new ChangeUserViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    CurrentProfileImageUrl = user.ProfileImageUrl
                };

                ViewBag.Message = "Profile updated successfully.";
                return View(updatedModel);
            }
            user.ProfileImageUrl = oldImageUrl;
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        /// <summary>
        /// Displays the password recovery page.
        /// </summary>
        /// <returns>The recover password view.</returns>
        [HttpGet]
        // GET: Account/RecoverPassword
        public IActionResult RecoverPassword()
        {
            return View();
        }

        /// <summary>
        /// Handles password recovery POST requests.
        /// </summary>
        /// <param name="model">The recover password view model.</param>
        /// <returns>The password confirmation view or the view with errors.</returns>
        [HttpPost]
        // POST: Account/RecoverPassword
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user != null)
                {
                    var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                    var encodedToken = UrlEncoder.Default.Encode(myToken);
                    var link = Url.Action("ResetPassword", "Account", new { token = encodedToken, email = user.Email }, protocol: Request.Scheme);
                    _mailHelper.SendEmail(model.Email, "Password Reset", $"<h1>Password Reset</h1>To reset your password click the link below:<br/><a href=\"{link}\">Reset Password</a>");
                }
                return View("RequestPasswordConfirmation");
            }
            return View(model);
        }

        /// <summary>
        /// Displays the reset password page.
        /// </summary>
        /// <param name="token">The password reset token.</param>
        /// <param name="email">The user's email address.</param>
        /// <returns>The reset password view or error view.</returns>
        [HttpGet]
        // GET: Account/ResetPassword
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                return View("Error");
            }
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        /// <summary>
        /// Displays the password reset confirmation page.
        /// </summary>
        /// <returns>The reset password confirmation view.</returns>
        [HttpGet]
        // GET: Account/ResetPasswordConfirmation
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Handles password reset POST requests.
        /// </summary>
        /// <param name="model">The reset password view model.</param>
        /// <returns>Redirects to confirmation or returns the view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Account/ResetPassword
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var decodedToken = System.Net.WebUtility.UrlDecode(model.Token);

            var result = await _userHelper.ResetPasswordAsync(user, decodedToken, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        /// <summary>
        /// Displays the change password page.
        /// </summary>
        /// <returns>The change password view.</returns>
        [HttpGet]
        // GET: Account/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Handles change password POST requests.
        /// </summary>
        /// <param name="model">The change password view model.</param>
        /// <returns>Redirects on success or returns the view with errors.</returns>
        [HttpPost]
        // POST: Account/ChangePassword
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ChangeUser");
                    }
                    ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault()?.Description ?? "Error changing password.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }
            return View(model);
        }

        /// <summary>
        /// Displays the not authorized page.
        /// </summary>
        /// <returns>The not authorized view.</returns>
        [HttpGet]
        // GET: Account/NotAuthorized
        public IActionResult NotAuthorized()
        {
            return View();
        }

        /// <summary>
        /// Creates a JWT token for API authentication.
        /// </summary>
        /// <param name="model">The login view model.</param>
        /// <returns>The JWT token or a bad request result.</returns>
        [HttpPost]
        // POST: Account/CreateToken
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userHelper.GetUserByEmailAsync(model.Username);
            if (user != null)
            {
                var result = await _userHelper.ValidatePasswordAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var claims = new[]
                    {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("ProfileImageUrl", user.ProfileImageUrl ?? "/images/default-profile.png")
                };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        _configuration["Tokens:Issuer"],
                        _configuration["Tokens:Audience"],
                        claims,
                        expires: DateTime.UtcNow.AddDays(15),
                        signingCredentials: credentials);

                    var results = new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    };

                    return Created(string.Empty, results);
                }
            }

            return BadRequest();
        }

        /// <summary>
        /// Generates a random password with the specified length.
        /// </summary>
        /// <param name="length">The length of the password.</param>
        /// <returns>A random password string.</returns>
        private static string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var password = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                var uintBuffer = new byte[sizeof(uint)];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    password.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }
            return password.ToString();
        }

        /// <summary>
        /// Confirms a user's email address using a token.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>The confirmation view with a message.</returns>
        [HttpGet]
        // GET: Account/ConfirmEmail
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ViewBag.Message = "Error: Invalid confirmation link. The link is missing required information.";
                return View();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                ViewBag.Message = "Error: User not found.";
                return View();
            }

            if (user.EmailConfirmed)
            {
                ViewBag.Message = "This email address has already been confirmed.";
                return View();
            }

            var decodedToken = System.Net.WebUtility.UrlDecode(token);
            var result = await _userHelper.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                ViewBag.Message = "Error: The confirmation link is invalid or has expired. Your email could not be confirmed.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName),
                new Claim("ProfileImageUrl", user.ProfileImageUrl ?? "/images/default-profile.png")
            };

            var roles = await _userHelper.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, claims);

            ViewBag.Message = "Your email address has been successfully confirmed!";
            return View();
        }
    }
}