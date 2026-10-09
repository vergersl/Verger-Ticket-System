using System.ComponentModel.DataAnnotations;

namespace VergerITDesk.Models.ViewModels;

public class TicketCreateVm
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string Category { get; set; } = TicketCategories.All[0];

    [Required]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    [Required(ErrorMessage = "Business impact / savings is required.")]
    public string BusinessImpact { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal? ImpactValueUsd { get; set; }
}

public class TicketIndexVm
{
    public List<Ticket> Tickets { get; set; } = new();
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public string? Query { get; set; }
    public bool IsRequesterView { get; set; }
}

public class TicketDetailVm
{
    public Ticket Ticket { get; set; } = null!;
    public bool CanManage { get; set; }
    public bool CanDelete { get; set; }
    public bool ShowRequestInfoPanel { get; set; }
    public string? MailtoLink { get; set; }
}

public class RequestInfoVm
{
    public int TicketId { get; set; }

    [Required, EmailAddress]
    public string To { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;
}
