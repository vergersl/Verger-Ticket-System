namespace VergerITDesk.Models.ViewModels;

public class KpiRowVm
{
    public KpiDefinition Definition { get; set; } = null!;
    public bool IsLive { get; set; }
    public decimal? CurrentMonthActual { get; set; }
}

public class KpiIndexVm
{
    public List<KpiRowVm> Rows { get; set; } = new();
    public string CurrentMonth { get; set; } = string.Empty;
}

public class KpiTrendVm
{
    public string Ref { get; set; } = string.Empty;
    public KpiDefinition Definition { get; set; } = null!;
    public List<(string Month, decimal? Actual)> Trend { get; set; } = new();
}
