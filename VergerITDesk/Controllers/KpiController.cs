using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VergerITDesk.Data;
using VergerITDesk.Models;
using VergerITDesk.Models.ViewModels;
using VergerITDesk.Services;

namespace VergerITDesk.Controllers;

[Authorize(Policy = "TechnicianOrAdmin")]
public class KpiController : Controller
{
    private readonly ApplicationDbContext _db;
    public KpiController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var defs = await _db.KpiDefinitions.OrderBy(d => d.Owner).ThenBy(d => d.Ref).ToListAsync();
        var month = KpiActual.CurrentFiscalMonth();
        var actuals = await _db.KpiActuals.Where(a => a.Month == month).ToDictionaryAsync(a => a.KpiRef);

        var tickets = await _db.Tickets.ToListAsync();
        var metrics = TicketMetricsService.Compute(tickets);

        var rows = defs.Select(d =>
        {
            decimal? actual;
            if (d.ComputedType == "sla") actual = metrics.SlaRate;
            else if (d.ComputedType == "fcr") actual = metrics.FcrRate;
            else actual = actuals.TryGetValue(d.Ref, out var a) ? a.Value : null;

            return new KpiRowVm { Definition = d, IsLive = d.ComputedType != null, CurrentMonthActual = actual };
        }).ToList();

        return View(new KpiIndexVm { Rows = rows, CurrentMonth = month });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveActual(string kpiRef, string month, decimal? percentValue)
    {
        var def = await _db.KpiDefinitions.FindAsync(kpiRef);
        if (def is null || def.ComputedType != null) return NotFound(); // live KPIs aren't manually editable

        var existing = await _db.KpiActuals.FirstOrDefaultAsync(a => a.KpiRef == kpiRef && a.Month == month);
        var fraction = percentValue.HasValue ? percentValue.Value / 100m : (decimal?)null;

        if (existing is null)
            _db.KpiActuals.Add(new KpiActual { KpiRef = kpiRef, Month = month, Value = fraction });
        else
            existing.Value = fraction;

        await _db.SaveChangesAsync();
        TempData["Toast"] = $"{kpiRef} · {month} actual saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Trend(string kpiRef)
    {
        var def = await _db.KpiDefinitions.FindAsync(kpiRef);
        if (def is null) return NotFound();

        var trend = new List<(string, decimal?)>();

        if (def.ComputedType != null)
        {
            var tickets = await _db.Tickets.Where(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)
                .ToListAsync();
            foreach (var month in KpiActual.FiscalMonths)
            {
                var monthTickets = tickets.Where(t => t.ResolvedAt.HasValue
                    && TicketMetricsService.FiscalIndex(t.ResolvedAt.Value) == Array.IndexOf(KpiActual.FiscalMonths, month))
                    .ToList();
                decimal? val = null;
                if (monthTickets.Count > 0)
                {
                    if (def.ComputedType == "sla")
                        val = (decimal)monthTickets.Count(t => t.ResolvedWithinSla()) / monthTickets.Count;
                    else
                        val = (decimal)monthTickets.Count(t => t.ReassignCount == 0 && !t.Reopened) / monthTickets.Count;
                }
                trend.Add((month, val));
            }
        }
        else
        {
            var actuals = await _db.KpiActuals.Where(a => a.KpiRef == kpiRef).ToDictionaryAsync(a => a.Month);
            foreach (var month in KpiActual.FiscalMonths)
                trend.Add((month, actuals.TryGetValue(month, out var a) ? a.Value : null));
        }

        return View(new KpiTrendVm { Ref = kpiRef, Definition = def, Trend = trend });
    }
}
