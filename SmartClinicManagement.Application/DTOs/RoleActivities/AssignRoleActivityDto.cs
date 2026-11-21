namespace SmartClinicManagement.Application.DTOs.RoleActivities;

using System.ComponentModel.DataAnnotations;

public class AssignRoleActivityDto
{
    [Required]
    public int RoleID { get; set; }

    [Required]
    public int ActivityID { get; set; }

    public int? AssignedBy { get; set; }
}