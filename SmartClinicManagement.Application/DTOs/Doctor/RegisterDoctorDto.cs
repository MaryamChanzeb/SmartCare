namespace SmartClinicManagement.Application.DTOs.Doctors;

/// <summary>
/// Single-step doctor registration: creates the User account and the
/// Doctor profile together, so the admin never has to pick an existing
/// user from a dropdown.
/// </summary>
public class RegisterDoctorDto
{
    // Account info (goes into Users)
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int? DepartmentID { get; set; }

    // Professional info (goes into Doctors)
    public string LicenseNumber { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string? Qualification { get; set; }

    public int ExperienceYears { get; set; }

    public decimal ConsultationFee { get; set; }

    public string? RoomNumber { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public string Status { get; set; } = "Active";
}
