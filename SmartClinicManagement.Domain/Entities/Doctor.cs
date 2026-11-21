using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class Doctor : BaseEntity
{
    public int DoctorID { get; set; }

    public int UserID { get; set; }

    public string LicenseNumber { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string? Qualification { get; set; }

    public int ExperienceYears { get; set; }

    public decimal ConsultationFee { get; set; }

    public string? RoomNumber { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public string Status { get; set; } = "Active";

    // Navigation Property
    public User? User { get; set; }
}