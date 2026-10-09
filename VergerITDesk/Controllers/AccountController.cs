using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VergerITDesk.Models;
using VergerITDesk.Models.ViewModels;

namespace VergerITDesk.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginVm { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
            return LocalRedirect(string.IsNullOrEmpty(vm.ReturnUrl) ? "/" : vm.ReturnUrl);

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(vm);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            DisplayName = vm.DisplayName
        };

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(vm);
        }

        // Bootstrap: the very first person to register becomes Admin, so someone
        // can start assigning Technician/Admin roles from the Team page. Everyone
        // after that defaults to Requester.
        var isFirstUser = _userManager.Users.Count() == 1;
        await _userManager.AddToRoleAsync(user, isFirstUser ? AppRoles.Admin : AppRoles.Requester);

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
