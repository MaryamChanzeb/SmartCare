namespace SmartClinicManagement.Application.DTOs.RoleActivities;

public class RoleActivitiesByRoleDto
{
    public int ActivityID { get; set; }

    public string ActivityName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? MenuURL { get; set; }

    public string Status { get; set; } = string.Empty;
}