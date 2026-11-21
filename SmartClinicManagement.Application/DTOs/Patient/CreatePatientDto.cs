namespace SmartClinicManagement.Application.DTOs.Patients;

public class CreatePatientDto
{
    public int UserID { get; set; }

    public string? BloodGroup { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalHistory { get; set; }

    public string Status { get; set; } = "Active";

    // Doctor primarily responsible for this patient (nullable — not every
    // patient may have one assigned yet).
    public int? PrimaryDoctorID { get; set; }
}