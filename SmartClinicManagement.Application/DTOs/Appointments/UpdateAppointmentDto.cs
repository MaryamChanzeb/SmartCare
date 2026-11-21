using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Appointments;

public class UpdateAppointmentDto
{
    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string? AppointmentType { get; set; }

    public string Status { get; set; } = "Scheduled";

    public string? Notes { get; set; }
}