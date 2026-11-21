using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Appointments;

    public class BookAppointmentDto
{
    public int DoctorID { get; set; }

    public int PatientID { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string? AppointmentType { get; set; }

    public string? Notes { get; set; }
}