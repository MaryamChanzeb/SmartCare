namespace SmartClinicManagement.Application.DTOs.Permissions;

public class PermissionDto
{
    public int PermissionID { get; set; }

    public int RoleActivityID { get; set; }

    public int RoleID { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int ActivityID { get; set; }

    public string ActivityName { get; set; } = string.Empty;

    public bool CanCreate { get; set; }

    public bool CanRead { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }
}