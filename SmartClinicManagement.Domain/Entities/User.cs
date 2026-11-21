using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class User : BaseEntity
{
    public int UserID { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;


    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string Status { get; set; } = "Active";

    public DateOnly CreatedAt { get; set; }

    public int? DepartmentID { get; set; }

    public int? RoleID { get; set; }
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public Department? Department { get; set; }

    public Role? Role { get; set; }
    public ICollection<PasswordDetails> PasswordDetails { get; set; }
    = new List<PasswordDetails>();
}