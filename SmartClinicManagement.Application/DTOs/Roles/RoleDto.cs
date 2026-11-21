namespace SmartClinicManagement.Application.DTOs.Roles;

public class RoleDto
{
    public int RoleID { get; set; }

    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}