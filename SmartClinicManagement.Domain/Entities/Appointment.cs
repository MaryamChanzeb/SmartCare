using SmartClinicManagement.Domain.Common;

public class Appointment : BaseEntity
{
    public int AppointmentID { get; set; }

    public int DoctorID { get; set; }

    public int PatientID { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly AppointmentTime { get; set; }

    public string? AppointmentType { get; set; }

    public string Status { get; set; } = "Scheduled";

    public string? Notes { get; set; }
}