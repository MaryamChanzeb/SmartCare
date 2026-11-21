using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Prescriptions;

public class CreatePrescriptionDto
{
    [Required]
    public int RecordID { get; set; }

    [Required]
    public string MedicineName { get; set; } = string.Empty;

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    public string? Duration { get; set; }

    public string? Instructions { get; set; }
}