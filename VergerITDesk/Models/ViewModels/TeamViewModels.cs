namespace VergerITDesk.Models.ViewModels;

public class TeamMemberVm
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = AppRoles.Requester;
    public bool IsMe { get; set; }
}
