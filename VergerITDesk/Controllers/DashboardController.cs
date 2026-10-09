using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VergerITDesk.Data;
using VergerITDesk.Models;
using VergerITDesk.Models.ViewModels;
using VergerITDesk.Services;

namespace VergerITDesk.Controllers;

[Authorize(Policy = "TechnicianOrAdmin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    public DashboardController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var tickets = await _db.Tickets.Include(t => t.Requester).ToListAsync();
        var metrics = TicketMetricsService.Compute(tickets);

        var byPriority = Enum.GetValues<TicketPriority>()
            .ToDictionary(p => p, p => tickets.Count(t => t.Priority == p));

        var byCategory = tickets.GroupBy(t => t.Category)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        var days = new List<(string, int, int)>();
        for (int i = 13; i >= 0; i--)
        {
            var day = DateTime.UtcNow.Date.AddDays(-i);
            var raised = tickets.Count(t => t.CreatedAt.Date == day);
            var resolved = tickets.Count(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == day);
            days.Add((day.ToString("ddd d"), raised, resolved));
        }

        var vm = new DashboardVm
        {
            Metrics = metrics,
            ByPriority = byPriority,
            ByCategory = byCategory,
            Last14Days = days,
            Recent = tickets.OrderByDescending(t => t.CreatedAt).Take(8).ToList()
        };

        return View(vm);
    }
}
