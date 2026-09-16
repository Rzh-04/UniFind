using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniversityLostAndFound.Models;
using UniversityLostAndFound.ViewModels;

namespace UniversityLostAndFound.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Department = model.Department,
                    StudentOrStaffId = model.StudentOrStaffId,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign default role "User"
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["SuccessMessage"] = $"Welcome to University Lost & Found, {user.FullName}!";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Logged in successfully!";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["InfoMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(new AccountProfileViewModel
            {
                FullName = user.FullName
            });
        }

        // POST: /Account/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AccountProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var changingPassword = !string.IsNullOrWhiteSpace(model.CurrentPassword)
                || !string.IsNullOrWhiteSpace(model.NewPassword)
                || !string.IsNullOrWhiteSpace(model.ConfirmNewPassword);

            if (changingPassword && string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Enter your current password to change it.");
            }

            if (changingPassword && string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Enter a new password.");
            }

            if (changingPassword && string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
            {
                ModelState.AddModelError(nameof(model.ConfirmNewPassword), "Confirm your new password.");
            }

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName.Trim();
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    AddIdentityErrors(updateResult);
                }
                else if (changingPassword)
                {
                    var passwordResult = await _userManager.ChangePasswordAsync(
                        user,
                        model.CurrentPassword!,
                        model.NewPassword!);

                    if (!passwordResult.Succeeded)
                    {
                        AddIdentityErrors(passwordResult);
                    }
                    else
                    {
                        await _signInManager.RefreshSignInAsync(user);
                    }
                }

                if (ModelState.IsValid)
                {
                    TempData["SuccessMessage"] = changingPassword
                        ? "Your name and password have been updated."
                        : "Your profile has been updated.";
                    return RedirectToAction(nameof(Profile));
                }
            }

            return View(model);
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
