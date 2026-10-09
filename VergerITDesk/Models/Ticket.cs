using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VergerITDesk.Models;

public class Ticket
{
    public int Id { get; set; }

    /// <summary>Human-friendly sequence number, shown as TKT-0001 etc.</summary>
    public int Seq { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, MaxLength(50)]
    public string Category { get; set; } = TicketCategories.All[0];

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    [Required]
    public string RequesterId { get; set; } = string.Empty;
    public ApplicationUser? Requester { get; set; }

    [MaxLength(100)]
    public string? AssignedTo { get; set; }

    public int SlaHours { get; set; }

    [Required(ErrorMessage = "Business impact / savings is required.")]
    public string BusinessImpact { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ImpactValueUsd { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public int ReassignCount { get; set; }
    public bool Reopened { get; set; }

    /// <summary>When the ticket most recently entered "Awaiting Info" (null if not currently paused).</summary>
    public DateTime? AwaitingInfoSince { get; set; }

    /// <summary>Total time (ticks) the SLA clock has spent paused across all "Awaiting Info" periods.</summary>
    public long PausedTicks { get; set; }

    public List<TicketComment> Comments { get; set; } = new();

    public static readonly Dictionary<TicketPriority, int> SlaHoursByPriority = new()
    {
        [TicketPriority.Critical] = 4,
        [TicketPriority.High] = 8,
        [TicketPriority.Medium] = 24,
        [TicketPriority.Low] = 72
    };

    public string Code => $"TKT-{Seq:D4}";

    [NotMapped]
    public TimeSpan PausedDuration => TimeSpan.FromTicks(PausedTicks);

    /// <summary>Elapsed working time excluding any time spent "Awaiting Info".</summary>
    public TimeSpan EffectiveElapsed()
    {
        var end = ResolvedAt ?? DateTime.UtcNow;
        var raw = end - CreatedAt;
        var paused = PausedDuration;
        if (AwaitingInfoSince.HasValue)
            paused += (end - AwaitingInfoSince.Value);
        var effective = raw - paused;
        return effective < TimeSpan.Zero ? TimeSpan.Zero : effective;
    }

    public bool ResolvedWithinSla()
    {
        if (Status != TicketStatus.Resolved && Status != TicketStatus.Closed) return false;
        if (ResolvedAt is null) return false;
        return EffectiveElapsed().TotalHours <= SlaHours;
    }
}
