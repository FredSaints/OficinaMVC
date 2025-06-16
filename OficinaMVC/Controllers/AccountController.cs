using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OficinaMVC.Data.Entities;
using OficinaMVC.Helpers;
using OficinaMVC.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OficinaMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _configuration;
        private readonly IImageHelper _imageHelper;

        public AccountController(IUserHelper userHelper, IMailHelper mailHelper, IConfiguration configuration, IImageHelper imageHelper)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _configuration = configuration;
            _imageHelper = imageHelper;
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

            var result = await _userHelper.LoginAsync(model);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

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

                //  If Mechanic, redirect to specialties/schedule setup ---
                if (model.Role == "Mechanic")
                {
                    TempData["SuccessMessage"] = "Mechanic created! Now configure specialties and schedule.";
                    return RedirectToAction("Edit", "Mechanics", new { id = user.Id });
                }

                // 3. Generate password reset token & link
                var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
                var link = Url.Action(
                    "ResetPassword", "Account",
                    new { token, userName = user.Email },
                    protocol: HttpContext.Request.Scheme);

                // 4. Send email to new user with link
                var response = _mailHelper.SendEmail(
                    user.Email,
                    "First access - set your password",
                    $"<h1>Welcome to Oficina!</h1><p>Your account was created by an administrator.<br>Click <a href='{link}'>here to set your password</a> before your first login.</p>"
                );

                ViewBag.Message = response.IsSuccess
                    ? "User created! An email was sent to the user to set their password."
                    : "User created, but the confirmation email could not be sent.";

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
                ViewBag.Message = "Profile updated successfully.";
                return View(model);
            }

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
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "The email does not correspond to a registered user.");
                    return View(model);
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                var link = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = myToken }, protocol: HttpContext.Request.Scheme);

                var response = _mailHelper.SendEmail(model.Email, "Password Reset", $"<h1>Password Reset</h1>" +
                    $"To reset your password click the link below:<br/><a href=\"{link}\">Reset Password</a>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "Instructions to recover your password have been sent to your email.";
                }

                return View();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userHelper.GetUserByEmailAsync(model.UserName);
            if (user != null)
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    if (!user.EmailConfirmed)
                    {
                        user.EmailConfirmed = true;
                        await _userHelper.UpdateUserAsync(user);
                    }
                    ViewBag.Message = "Password reset successful. You can now log in.";
                    return View();
                }
                ViewBag.Message = "Error while resetting the password.";
                return View(model);
            }

            ViewBag.Message = "User not found.";
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

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                ViewBag.Message = "Error confirming email.";
            }
            else
            {
                ViewBag.Message = "Email confirmed successfully!";
            }

            return View();
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
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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