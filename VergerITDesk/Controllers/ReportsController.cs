using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VergerITDesk.Data;
using VergerITDesk.Models;
using VergerITDesk.Models.ViewModels;
using VergerITDesk.Services;

namespace VergerITDesk.Controllers;

[Authorize(Policy = "TechnicianOrAdmin")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var tickets = await _db.Tickets.Include(t => t.Requester).ToListAsync();
        var metrics = TicketMetricsService.Compute(tickets);

        var raisedByMonth = new int[12];
        var resolvedByMonth = new int[12];
        foreach (var t in tickets)
        {
            raisedByMonth[TicketMetricsService.FiscalIndex(t.CreatedAt)]++;
            if (t.ResolvedAt.HasValue) resolvedByMonth[TicketMetricsService.FiscalIndex(t.ResolvedAt.Value)]++;
        }

        ViewBag.Metrics = metrics;
        ViewBag.Months = KpiActual.FiscalMonths;
        ViewBag.RaisedByMonth = raisedByMonth;
        ViewBag.ResolvedByMonth = resolvedByMonth;
        ViewBag.KpiDefs = await _db.KpiDefinitions.OrderBy(d => d.Owner).ThenBy(d => d.Ref).ToListAsync();

        return View();
    }

    public async Task<IActionResult> ExportCsv()
    {
        var tickets = await _db.Tickets.Include(t => t.Requester).OrderBy(t => t.Seq).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Ticket,Title,Status,Priority,Category,Requester,Requester Email,Assigned To,Created At,Resolved At,SLA Hours,Business Impact,Impact Value (USD)");
        foreach (var t in tickets)
        {
            sb.AppendLine(string.Join(",",
                Csv(t.Code), Csv(t.Title), Csv(t.Status.ToString()), Csv(t.Priority.ToString()), Csv(t.Category),
                Csv(t.Requester?.DisplayName ?? ""), Csv(t.Requester?.Email ?? ""), Csv(t.AssignedTo ?? ""),
                Csv(t.CreatedAt.ToString("O")), Csv(t.ResolvedAt?.ToString("O") ?? ""),
                Csv(t.SlaHours.ToString()), Csv(t.BusinessImpact), Csv(t.ImpactValueUsd?.ToString() ?? "")));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "verger-it-tickets.csv");
    }

    private static string Csv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
