using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class Prescription : BaseEntity
{
    public int PrescriptionID { get; set; }

    public int RecordID { get; set; }

    public string? MedicineName { get; set; }

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    public string? Duration { get; set; }

    public string? Instructions { get; set; }
}