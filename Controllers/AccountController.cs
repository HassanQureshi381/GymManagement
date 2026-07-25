using GymManagementSystem.Models;
using GymManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers;

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

    // ── GET: /Account/Login ───────────────────────────
    [HttpGet]
    public IActionResult Login()
    {
        // Redirect if already logged in
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    // ── POST: /Account/Login ──────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Step 1: Find user by email
        var user = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());
        if (user == null)
        {
            ModelState.AddModelError("", $"No user found with email: {model.Email}");
            return View(model);
        }

        // Step 2: Check password
        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            ModelState.AddModelError("", "Password is incorrect.");
            return View(model);
        }

        // Step 3: Sign in
        var result = await _signInManager.PasswordSignInAsync(
            userName: user.UserName!,
            password: model.Password,
            isPersistent: model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        ModelState.AddModelError("", $"SignIn failed. Locked: {result.IsLockedOut}, NotAllowed: {result.IsNotAllowed}");
        return View(model);
    }

    // ── POST: /Account/Logout ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    // ── Access Denied ─────────────────────────────────
    public IActionResult AccessDenied() => View();
}