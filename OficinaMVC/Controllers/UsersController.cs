using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Helpers;
using System.Security.Claims;

namespace OficinaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public UsersController(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        // GET: /Users
        public async Task<IActionResult> Index(string searchString, string searchType = "name")
        {
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usersQuery = _context.Users
                .Where(u => u.Id != currentAdminId);

            if (!string.IsNullOrEmpty(searchString))
            {
                switch (searchType.ToLower())
                {
                    case "nif":
                        usersQuery = usersQuery.Where(u => u.NIF.StartsWith(searchString));
                        break;
                    case "name":
                    default:
                        usersQuery = usersQuery.Where(u =>
                            (u.FirstName + " " + u.LastName).Contains(searchString));
                        break;
                }
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSearchType"] = searchType;

            var users = await usersQuery
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            return View(users);
        }

        // POST: /Users/Deactivate/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userToDeactivate = await _userHelper.GetUserByIdAsync(id);
            if (userToDeactivate == null)
            {
                return NotFound();
            }

            var result = await _userHelper.DeactivateUserAsync(userToDeactivate);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {userToDeactivate.FullName} has been successfully deactivated.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Could not deactivate user. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Users/Reactivate/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var userToReactivate = await _userHelper.GetUserByIdAsync(id);
            if (userToReactivate == null)
            {
                return NotFound();
            }

            var result = await _userHelper.ReactivateUserAsync(userToReactivate);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {userToReactivate.FullName} has been successfully reactivated.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Could not reactivate user. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}