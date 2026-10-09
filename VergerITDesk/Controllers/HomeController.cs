using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VergerITDesk.Models;

namespace VergerITDesk.Controllers;

public class HomeController : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        if (User.IsInRole(AppRoles.Technician) || User.IsInRole(AppRoles.Admin))
            return RedirectToAction("Index", "Dashboard");
        return RedirectToAction("Index", "Tickets");
    }

    [AllowAnonymous]
    public IActionResult Error() => View();
}
