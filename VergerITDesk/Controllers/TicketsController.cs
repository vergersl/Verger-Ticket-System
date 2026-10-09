using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web;
using VergerITDesk.Data;
using VergerITDesk.Models;
using VergerITDesk.Models.ViewModels;

namespace VergerITDesk.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TicketsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private bool IsRequesterOnly => User.IsInRole(AppRoles.Requester)
        && !User.IsInRole(AppRoles.Technician) && !User.IsInRole(AppRoles.Admin);

    // GET /Tickets?status=&priority=&category=&q=
    public async Task<IActionResult> Index(string? status, string? priority, string? category, string? q)
    {
        var userId = _userManager.GetUserId(User);
        var query = _db.Tickets.Include(t => t.Requester).AsQueryable();

        var requesterOnly = IsRequesterOnly;
        if (requesterOnly)
            query = query.Where(t => t.RequesterId == userId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatus>(status, out var st))
            query = query.Where(t => t.Status == st);
        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TicketPriority>(priority, out var pr))
            query = query.Where(t => t.Priority == pr);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(t => t.Category == category);
        if (!string.IsNullOrEmpty(q))
            query = query.Where(t => t.Title.Contains(q) || (t.Description != null && t.Description.Contains(q)));

        var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        return View(new TicketIndexVm
        {
            Tickets = tickets,
            Status = status,
            Priority = priority,
            Category = category,
            Query = q,
            IsRequesterView = requesterOnly
        });
    }

    [HttpGet]
    public IActionResult Create() => View(new TicketCreateVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TicketCreateVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var nextSeq = (await _db.Tickets.MaxAsync(t => (int?)t.Seq) ?? 0) + 1;
        var now = DateTime.UtcNow;

        var ticket = new Ticket
        {
            Seq = nextSeq,
            Title = vm.Title,
            Description = vm.Description,
            Category = vm.Category,
            Priority = vm.Priority,
            Status = TicketStatus.Open,
            RequesterId = user.Id,
            SlaHours = Ticket.SlaHoursByPriority[vm.Priority],
            BusinessImpact = vm.BusinessImpact,
            ImpactValueUsd = vm.ImpactValueUsd,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        TempData["Toast"] = $"Ticket {ticket.Code} submitted.";
        return RedirectToAction(nameof(Details), new { id = ticket.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _db.Tickets.Include(t => t.Requester).Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (IsRequesterOnly && ticket.RequesterId != userId) return Forbid();

        var canManage = User.IsInRole(AppRoles.Technician) || User.IsInRole(AppRoles.Admin);

        return View(new TicketDetailVm
        {
            Ticket = ticket,
            CanManage = canManage,
            CanDelete = User.IsInRole(AppRoles.Admin)
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "TechnicianOrAdmin")]
    public async Task<IActionResult> SaveEdits(int id, TicketStatus status, string? assignedTo)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();

        var now = DateTime.UtcNow;
        assignedTo ??= string.Empty;

        if (assignedTo != (ticket.AssignedTo ?? string.Empty))
        {
            if (!string.IsNullOrEmpty(ticket.AssignedTo)) ticket.ReassignCount++;
            ticket.AssignedTo = assignedTo;
        }

        if (status != ticket.Status)
        {
            if ((status == TicketStatus.Resolved || status == TicketStatus.Closed) && ticket.ResolvedAt is null)
                ticket.ResolvedAt = now;
            if (status == TicketStatus.Open && ticket.Status == TicketStatus.Resolved)
            {
                ticket.Reopened = true;
                ticket.ResolvedAt = null;
            }
            if (status == TicketStatus.AwaitingInfo && ticket.Status != TicketStatus.AwaitingInfo)
                ticket.AwaitingInfoSince = now;
            if (ticket.Status == TicketStatus.AwaitingInfo && status != TicketStatus.AwaitingInfo)
            {
                ticket.PausedTicks += Math.Max(0, (now - (ticket.AwaitingInfoSince ?? now)).Ticks);
                ticket.AwaitingInfoSince = null;
            }
            ticket.Status = status;
        }

        ticket.UpdatedAt = now;
        await _db.SaveChangesAsync();

        TempData["Toast"] = "Ticket updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "TechnicianOrAdmin")]
    public async Task<IActionResult> Resolve(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();

        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Toast"] = "Ticket marked resolved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "TechnicianOrAdmin")]
    public async Task<IActionResult> Reopen(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();

        ticket.Status = TicketStatus.Open;
        ticket.ResolvedAt = null;
        ticket.Reopened = true;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Toast"] = "Ticket reopened.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "TechnicianOrAdmin")]
    public async Task<IActionResult> Resume(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();

        var now = DateTime.UtcNow;
        if (ticket.AwaitingInfoSince.HasValue)
            ticket.PausedTicks += Math.Max(0, (now - ticket.AwaitingInfoSince.Value).Ticks);
        ticket.AwaitingInfoSince = null;
        ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = now;

        _db.TicketComments.Add(new TicketComment
        {
            TicketId = ticket.Id,
            Author = "IT team",
            Text = "Resumed work after requester response.",
            At = now
        });

        await _db.SaveChangesAsync();
        TempData["Toast"] = "Ticket resumed.";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Sets the ticket to Awaiting Info, logs the request, and hands back a mailto: link
    /// for the technician's own mail client — there's no email server behind this.</summary>
    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "TechnicianOrAdmin")]
    public async Task<IActionResult> RequestInfo(RequestInfoVm vm)
    {
        var ticket = await _db.Tickets.FindAsync(vm.TicketId);
        if (ticket is null) return NotFound();
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please fill in the recipient, subject and message.";
            return RedirectToAction(nameof(Details), new { id = vm.TicketId });
        }

        var now = DateTime.UtcNow;
        if (ticket.Status != TicketStatus.AwaitingInfo)
            ticket.AwaitingInfoSince = now;
        ticket.Status = TicketStatus.AwaitingInfo;
        ticket.UpdatedAt = now;

        _db.TicketComments.Add(new TicketComment
        {
            TicketId = ticket.Id,
            Author = "IT team",
            Text = $"Requested more information from {ticket.Requester?.DisplayName} ({vm.To}): \"{vm.Subject}\"",
            At = now
        });

        await _db.SaveChangesAsync();

        var mailto = $"mailto:{HttpUtility.UrlEncode(vm.To)}?subject={HttpUtility.UrlEncode(vm.Subject)}&body={HttpUtility.UrlEncode(vm.Body)}";
        TempData["Toast"] = "Status set to Awaiting Info.";
        TempData["MailtoLink"] = mailto;
        return RedirectToAction(nameof(Details), new { id = vm.TicketId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string text)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (IsRequesterOnly && ticket.RequesterId != userId) return Forbid();
        if (string.IsNullOrWhiteSpace(text)) return RedirectToAction(nameof(Details), new { id });

        var user = await _userManager.GetUserAsync(User);
        _db.TicketComments.Add(new TicketComment
        {
            TicketId = id,
            Author = user?.DisplayName ?? "Someone",
            Text = text.Trim(),
            At = DateTime.UtcNow
        });
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return NotFound();

        _db.Tickets.Remove(ticket); // cascades to comments
        await _db.SaveChangesAsync();

        TempData["Toast"] = $"Ticket {ticket.Code} deleted.";
        return RedirectToAction(nameof(Index));
    }
}
