using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class Patient : BaseEntity
{
    public int PatientID { get; set; }

    public int UserID { get; set; }

    public string? BloodGroup { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalHistory { get; set; }

    public string Status { get; set; } = "Active";

    // The doctor primarily responsible for this patient, assigned at registration time by the receptionist/admin. Used to scope a doctor's patient list to only their own assigned patients.
    public int? PrimaryDoctorID { get; set; }

    // Navigation Property
    public User? User { get; set; }
    public Doctor? PrimaryDoctor { get; set; }
}