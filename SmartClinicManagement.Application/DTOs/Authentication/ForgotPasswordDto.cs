using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Authentication;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
