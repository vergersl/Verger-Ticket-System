using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VergerITDesk.Models;
using VergerITDesk.Models.ViewModels;

namespace VergerITDesk.Controllers;

[Authorize(Policy = "AdminOnly")]
public class TeamController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    public TeamController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<IActionResult> Index()
    {
        var myId = _userManager.GetUserId(User);
        var members = new List<TeamMemberVm>();

        foreach (var user in _userManager.Users.ToList())
        {
            var roles = await _userManager.GetRolesAsync(user);
            // A user can technically hold more than one role via direct Identity calls;
            // this app only ever assigns one, so pick the highest-privilege one to display.
            var role = roles.Contains(AppRoles.Admin) ? AppRoles.Admin
                : roles.Contains(AppRoles.Technician) ? AppRoles.Technician
                : AppRoles.Requester;

            members.Add(new TeamMemberVm
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? "",
                Role = role,
                IsMe = user.Id == myId
            });
        }

        return View(members.OrderBy(m => m.Role == AppRoles.Requester).ThenBy(m => m.DisplayName));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        if (!AppRoles.All.Contains(role)) return BadRequest();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

        await _userManager.AddToRoleAsync(user, role);

        TempData["Toast"] = $"{user.DisplayName} is now {role}.";
        return RedirectToAction(nameof(Index));
    }
}
