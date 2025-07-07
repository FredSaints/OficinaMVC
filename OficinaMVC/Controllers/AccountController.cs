using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Encodings;
using OficinaMVC.Data.Entities;
using OficinaMVC.Helpers;
using OficinaMVC.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace OficinaMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _configuration;
        private readonly IImageHelper _imageHelper;
        private readonly SignInManager<User> _signInManager;

        public AccountController(IUserHelper userHelper, IMailHelper mailHelper, IConfiguration configuration, IImageHelper imageHelper, SignInManager<User> signInManager)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _configuration = configuration;
            _imageHelper = imageHelper;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }



        [HttpPost]
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
                new Claim("FullName", user.FullName),
                new Claim("ProfileImageUrl", user.ProfileImageUrl ?? "/images/default-profile.png")
            };

                    var userRoles = await _userHelper.GetRolesAsync(user);

                    foreach (var role in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    await _userHelper.SignInWithClaimsAsync(user, model.RememberMe, claims);

                    // Correct Redirect Logic
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

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            ViewBag.Roles = RolesHelper.Roles;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
                ProfileImageUrl = imageUrl
            };

            // 1. Generate random password
            var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 10);
            var result = await _userHelper.AddUserAsync(user, tempPassword);

            if (result.Succeeded)
            {
                // 2. Assign selected role
                await _userHelper.CheckRoleAsync(model.Role);
                await _userHelper.AddUserToRoleAsync(user, model.Role);

                // 3. Generate password reset token & link (for ALL users)
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = UrlEncoder.Default.Encode(token);
                var link = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, token = encodedToken },
                    protocol: HttpContext.Request.Scheme);

                // 4. Send email to user
                var response = _mailHelper.SendEmail(
                    user.Email,
                    "Confirm Your Account & Set Your Password - FredAuto",
                    $"<h1>Welcome to FredAuto!</h1>" +
                    $"<p>Your account has been created by an administrator. Please confirm your account and set your initial password by clicking the link below:</p>" +
                    $"<p><a href='{link}'>Set My Password</a></p>"
                );

                ViewBag.Message = response.IsSuccess
                    ? "User created! An email was sent to the user to set their password."
                    : "User created, but the confirmation email could not be sent.";

                // 5. Mechanic-specific redirect (after email is sent)
                if (model.Role == "Mechanic")
                {
                    TempData["SuccessMessage"] = "Mechanic created! Now configure specialties and schedule.";
                    return RedirectToAction("Edit", "Mechanics", new { id = user.Id });
                }

                return View("RegisterConfirmation");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Roles = RolesHelper.Roles;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return NotFound();

            var model = new ChangeUserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (user == null) return NotFound();

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

        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
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

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                return View("Error");
            }
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
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

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return View("Error");
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }

            // FIX: Properly decode token
            var decodedToken = System.Net.WebUtility.UrlDecode(token);

            var result = await _userHelper.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                ViewBag.Message = "Error: The confirmation link is invalid or has expired.";
                return View();
            }

            var resetToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
            var encodedResetToken = UrlEncoder.Default.Encode(resetToken);

            return RedirectToAction("ResetPassword", new { token = encodedResetToken, email = user.Email });
        }

        [HttpGet]
        public IActionResult NotAuthorized()
        {
            return View();
        }

        [HttpPost]
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
    }
}