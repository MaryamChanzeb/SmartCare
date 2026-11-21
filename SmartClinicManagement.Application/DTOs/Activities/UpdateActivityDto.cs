using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Activities;

public class UpdateActivityDto
{
    [Required]
    [StringLength(100)]
    public string ActivityName { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? MenuURL { get; set; }

    [Required]
    [RegularExpression("Active|Inactive")]
    public string Status { get; set; } = "Active";
}