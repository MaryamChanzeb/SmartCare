namespace SmartClinicManagement.Application.DTOs.RoleActivities;

public class RoleActivityDto
{
    public int RoleActivityID { get; set; }

    public int RoleID { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int ActivityID { get; set; }

    public string ActivityName { get; set; } = string.Empty;

    public DateTime? AssignedDate { get; set; }

    public int? AssignedBy { get; set; }

    public string Status { get; set; } = string.Empty;
}