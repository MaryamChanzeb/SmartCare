using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class MedicalRecord : BaseEntity
{
    public int RecordID { get; set; }

    public int PatientID { get; set; }

    public int DoctorID { get; set; }

    public string? Diagnosis { get; set; }

    public string? Symptoms { get; set; }

    public string? Treatment { get; set; }

    public string? Notes { get; set; }

    public DateTime? VisitDate { get; set; }
}