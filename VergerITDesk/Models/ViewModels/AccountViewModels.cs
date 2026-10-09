using System.ComponentModel.DataAnnotations;

namespace VergerITDesk.Models.ViewModels;

public class LoginVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterVm
{
    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords don't match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
