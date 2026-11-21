namespace SmartClinicManagement.Application.DTOs.Patients;

/// <summary>
/// Single-step patient registration: creates the User account and the
/// Patient profile together, so the receptionist never has to pick an
/// existing user from a dropdown.
/// </summary>
public class RegisterPatientDto
{
    // Account info (goes into Users)
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    // Clinical info (goes into Patients)
    public string? BloodGroup { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalHistory { get; set; }

    public string Status { get; set; } = "Active";

    // Doctor assigned to this patient at registration time.
    public int? DoctorID { get; set; }
}
