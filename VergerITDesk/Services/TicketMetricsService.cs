using VergerITDesk.Models;

namespace VergerITDesk.Services;

public class TicketMetrics
{
    public int Total { get; set; }
    public int Open { get; set; }
    public int AwaitingInfo { get; set; }
    public int ResolvedCount { get; set; }
    public decimal? SlaRate { get; set; }
    public decimal? FcrRate { get; set; }
    public double? AvgResolutionHours { get; set; }
    public decimal TotalImpactUsd { get; set; }
    public int QuantifiedCount { get; set; }
}

public static class TicketMetricsService
{
    public static TicketMetrics Compute(IReadOnlyCollection<Ticket> tickets)
    {
        var resolved = tickets.Where(t => t.Status is TicketStatus.Resolved or TicketStatus.Closed).ToList();

        var withinSla = resolved.Where(t => t.ResolvedWithinSla()).ToList();
        decimal? slaRate = resolved.Count > 0 ? (decimal)withinSla.Count / resolved.Count : null;

        // "Eligible" for first-contact resolution mirrors tickets that have a meaningful reassignment count.
        var eligible = resolved; // ReassignCount defaults to 0, so every resolved ticket is eligible.
        var fcr = eligible.Where(t => t.ReassignCount == 0 && !t.Reopened).ToList();
        decimal? fcrRate = eligible.Count > 0 ? (decimal)fcr.Count / eligible.Count : null;

        var open = tickets.Count(t => t.Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.AwaitingInfo);
        var awaiting = tickets.Count(t => t.Status == TicketStatus.AwaitingInfo);

        var resTimes = resolved.Where(t => t.ResolvedAt.HasValue).Select(t => t.EffectiveElapsed().TotalHours).ToList();
        double? avgRes = resTimes.Count > 0 ? resTimes.Average() : null;

        var quantified = resolved.Where(t => t.ImpactValueUsd.HasValue).ToList();
        var totalImpact = quantified.Sum(t => t.ImpactValueUsd ?? 0m);

        return new TicketMetrics
        {
            Total = tickets.Count,
            Open = open,
            AwaitingInfo = awaiting,
            ResolvedCount = resolved.Count,
            SlaRate = slaRate,
            FcrRate = fcrRate,
            AvgResolutionHours = avgRes,
            TotalImpactUsd = totalImpact,
            QuantifiedCount = quantified.Count
        };
    }

    /// <summary>Fiscal-month index (0=Apr .. 11=Mar) for a UTC timestamp.</summary>
    public static int FiscalIndex(DateTime utc) => (utc.Month + 8) % 12;
}
