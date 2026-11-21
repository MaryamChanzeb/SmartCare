using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Appointments;

public class AppointmentDto
{
    public int AppointmentID { get; set; }

    public int DoctorID { get; set; }

    public int PatientID { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string? AppointmentType { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }
}