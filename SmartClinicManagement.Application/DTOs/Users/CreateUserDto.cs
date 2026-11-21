namespace SmartClinicManagement.Application.DTOs.Users;

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int? DepartmentID { get; set; }

    public int? RoleID { get; set; }
}
