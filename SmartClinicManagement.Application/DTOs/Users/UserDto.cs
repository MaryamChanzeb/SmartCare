namespace SmartClinicManagement.Application.DTOs.Users;

public class UserDto
{
    public int UserID { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? DepartmentName { get; set; }

    public string? RoleName { get; set; }
}