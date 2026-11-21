using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Authentication;

public class RegisterDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    // Must be one of "Patient", "Doctor", "Receptionist" (case-insensitive).
    // Public registration can never grant Admin/SuperAdmin — that is
    // enforced server-side in AuthService, not just by the UI dropdown.
    [Required]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    // Only used when RoleName == "Doctor". Ignored otherwise.
    public string? LicenseNumber { get; set; }

    public string? Specialization { get; set; }
}
