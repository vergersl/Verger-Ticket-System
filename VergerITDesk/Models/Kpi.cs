using System.ComponentModel.DataAnnotations;

namespace VergerITDesk.Models;

/// <summary>Static reference row from the Digital &amp; IT KPI Master Sheet. Seeded once; not user-editable.</summary>
public class KpiDefinition
{
    [Key, MaxLength(20)]
    public string Ref { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Csf { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Owner { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Role { get; set; } = string.Empty;

    public int Weight { get; set; }

    /// <summary>Target as a fraction, e.g. 0.90 for 90%.</summary>
    public decimal Target { get; set; }

    public string Formula { get; set; } = string.Empty;

    /// <summary>"sla" | "fcr" | null — null means the KPI is tracked manually via <see cref="KpiActual"/>.</summary>
    [MaxLength(10)]
    public string? ComputedType { get; set; }
}

/// <summary>One month's manually-entered actual for a KPI. Fiscal months: Apr..Mar.</summary>
public class KpiActual
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string KpiRef { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string Month { get; set; } = string.Empty; // "Apr".."Mar"

    /// <summary>Actual value as a fraction, e.g. 0.92 for 92%. Null = no data entered.</summary>
    public decimal? Value { get; set; }

    public static readonly string[] FiscalMonths =
        { "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar" };

    public static string CurrentFiscalMonth()
    {
        // Calendar month (1=Jan) mapped onto the Apr-start fiscal year.
        int m = DateTime.UtcNow.Month; // 1..12
        int idx = (m + 8) % 12;        // Apr(4)->0 ... Mar(3)->11
        return FiscalMonths[idx];
    }
}
