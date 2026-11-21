using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Authentication;

public class ChangePasswordDto
{
    public int UserID { get; set; }

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}