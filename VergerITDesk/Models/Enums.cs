namespace VergerITDesk.Models;

public enum TicketStatus
{
    Open,
    InProgress,
    AwaitingInfo,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>App-level roles, distinct from any other Identity roles you might add later.</summary>
public static class AppRoles
{
    public const string Requester = "Requester";
    public const string Technician = "Technician";
    public const string Admin = "Admin";

    public static readonly string[] All = { Requester, Technician, Admin };
}

public static class TicketCategories
{
    public static readonly string[] All =
    {
        "Hardware", "Software", "Network", "ERP / Acumatica",
        "BI / Dashboards", "Access & Accounts", "Security", "Other"
    };
}
