
using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.DTOs.Appointments;
public interface IAppointmentService
{
    // Returns the real outcome of the confirmation email sent after a successful booking.
    Task<EmailSendResult> BookAppointmentAsync(BookAppointmentDto dto);

    // Null when the update was neither a reschedule nor a fresh
    // confirmation — no notification email is sent in that case, so there
    // is no delivery outcome to report.
    Task<EmailSendResult?> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto);

    Task<EmailSendResult> ConfirmAppointmentAsync(int id);

    Task<EmailSendResult> CancelAppointmentAsync(int id);

    Task<AppointmentDto?> GetAppointmentByIdAsync(int id);

    Task<List<AppointmentDto>> GetAllAppointmentsAsync();

}