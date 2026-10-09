using VergerITDesk.Services;

namespace VergerITDesk.Models.ViewModels;

public class DashboardVm
{
    public TicketMetrics Metrics { get; set; } = new();
    public Dictionary<TicketPriority, int> ByPriority { get; set; } = new();
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public List<(string Label, int Raised, int Resolved)> Last14Days { get; set; } = new();
    public List<Ticket> Recent { get; set; } = new();
}
