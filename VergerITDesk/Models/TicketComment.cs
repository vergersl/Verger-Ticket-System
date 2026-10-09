using System.ComponentModel.DataAnnotations;

namespace VergerITDesk.Models;

public class TicketComment
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    [Required, MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [Required]
    public string Text { get; set; } = string.Empty;

    public DateTime At { get; set; } = DateTime.UtcNow;
}
