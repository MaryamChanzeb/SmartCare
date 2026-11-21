using Microsoft.Extensions.Logging;
using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.DTOs.Appointments;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using SmartClinicManagement.Infrastructure.Helpers;

namespace SmartClinicManagement.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<AppointmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }


    public async Task<List<AppointmentDto>> GetAllAppointmentsAsync()
    {
        var appointments = await _unitOfWork
            .Repository<Appointment>()
            .GetAllAsync(x => !x.IsDeleted);

        return appointments.Select(a => new AppointmentDto
        {
            AppointmentID = a.AppointmentID,
            DoctorID = a.DoctorID,
            PatientID = a.PatientID,
            AppointmentDate = a.AppointmentDate,
            AppointmentTime = a.AppointmentTime,
            AppointmentType = a.AppointmentType,
            Status = a.Status,
            Notes = a.Notes
        }).ToList();
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
    {
        var appointment = await _unitOfWork
            .Repository<Appointment>()
            .FirstOrDefaultAsync(x => x.AppointmentID == id && !x.IsDeleted);

        if (appointment == null)
            return null;

        return new AppointmentDto
        {
            AppointmentID = appointment.AppointmentID,
            DoctorID = appointment.DoctorID,
            PatientID = appointment.PatientID,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime,
            AppointmentType = appointment.AppointmentType,
            Status = appointment.Status,
            Notes = appointment.Notes
        };
    }

    public async Task<EmailSendResult> BookAppointmentAsync(BookAppointmentDto dto)
    {
        var doctorExists = await _unitOfWork
            .Repository<Doctor>()
            .AnyAsync(x => x.DoctorID == dto.DoctorID && !x.IsDeleted);

        if (!doctorExists)
            throw new Exception("Doctor not found.");

        var patientExists = await _unitOfWork
            .Repository<Patient>()
            .AnyAsync(x => x.PatientID == dto.PatientID && !x.IsDeleted);

        if (!patientExists)
            throw new Exception("Patient not found.");

        if (dto.AppointmentDate < DateOnly.FromDateTime(DateTime.Today))
            throw new Exception("Appointment date cannot be in the past.");

        var slotBooked = await _unitOfWork
            .Repository<Appointment>()
            .AnyAsync(x =>
                x.DoctorID == dto.DoctorID &&
                x.AppointmentDate == dto.AppointmentDate &&
                x.AppointmentTime == dto.AppointmentTime &&
                x.Status != "Cancelled" &&
                !x.IsDeleted);

        if (slotBooked)
            throw new Exception("Doctor already has an appointment at this time.");

        var appointment = new Appointment
        {
            DoctorID = dto.DoctorID,
            PatientID = dto.PatientID,
            AppointmentDate = dto.AppointmentDate,
            AppointmentTime = dto.AppointmentTime,
            AppointmentType = dto.AppointmentType,
            Notes = dto.Notes,
            Status = "Scheduled"
        };

        await _unitOfWork
            .Repository<Appointment>()
            .AddAsync(appointment);

        await _unitOfWork.SaveChangesAsync();

        // Booking itself already succeeded above — only the email outcome
        // is reported here, without ever undoing the booking.
        return await SendAppointmentEmailAsync(
            appointment,
            "Appointment Confirmation",
            "Your appointment has been booked successfully.");
    }

    public async Task<EmailSendResult?> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _unitOfWork
            .Repository<Appointment>()
            .GetByIdAsync(id);

        if (appointment == null || appointment.IsDeleted)
            throw new Exception("Appointment not found.");

        var slotBooked = await _unitOfWork
            .Repository<Appointment>()
            .AnyAsync(x =>
                x.DoctorID == appointment.DoctorID &&
                x.AppointmentDate == dto.AppointmentDate &&
                x.AppointmentTime == dto.AppointmentTime &&
                x.AppointmentID != id &&
                x.Status != "Cancelled" &&
                !x.IsDeleted);

        if (slotBooked)
            throw new Exception("Doctor already has an appointment at this time.");

        // Reschedule = date and/or time actually changing on an existing appointment.
        var isReschedule =
            appointment.AppointmentDate != dto.AppointmentDate ||
            appointment.AppointmentTime != dto.AppointmentTime;

        // The UI's "Confirm" action goes through this same generic update
        // endpoint (only sending Status = "Confirmed"), rather than the
        // dedicated /confirm endpoint below, so that transition is detected
        // here too.
        var isNewlyConfirmed =
            appointment.Status != "Confirmed" &&
            dto.Status == "Confirmed";

        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.AppointmentTime = dto.AppointmentTime;
        appointment.AppointmentType = dto.AppointmentType;
        appointment.Status = dto.Status;
        appointment.Notes = dto.Notes;
        appointment.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork
            .Repository<Appointment>()
            .UpdateAsync(appointment);

        await _unitOfWork.SaveChangesAsync();

        if (isReschedule)
        {
            return await SendAppointmentEmailAsync(
                appointment,
                "Appointment Rescheduled",
                "Your appointment has been rescheduled. Please find the updated details below.");
        }

        if (isNewlyConfirmed)
        {
            return await SendAppointmentEmailAsync(
                appointment,
                "Appointment Confirmed",
                "Your appointment has been confirmed successfully.");
        }

        // No notification applies to this particular update — there is no
        // email outcome to report.
        return null;
    }
    public async Task<EmailSendResult> ConfirmAppointmentAsync(int id)
    {
        var appointment = await _unitOfWork
            .Repository<Appointment>()
            .GetByIdAsync(id);

        if (appointment == null || appointment.IsDeleted)
            throw new Exception("Appointment not found.");

        appointment.Status = "Confirmed";
        appointment.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork
            .Repository<Appointment>()
            .UpdateAsync(appointment);

        await _unitOfWork.SaveChangesAsync();

        return await SendAppointmentEmailAsync(
            appointment,
            "Appointment Confirmed",
            "Your appointment has been confirmed successfully.");
    }
    public async Task<EmailSendResult> CancelAppointmentAsync(int id)
    {
        var appointment = await _unitOfWork
            .Repository<Appointment>()
            .GetByIdAsync(id);

        if (appointment == null || appointment.IsDeleted)
            throw new Exception("Appointment not found.");

        appointment.Status = "Cancelled";
        appointment.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork
            .Repository<Appointment>()
            .UpdateAsync(appointment);

        await _unitOfWork.SaveChangesAsync();

        return await SendAppointmentEmailAsync(
            appointment,
            "Appointment Cancelled",
            "Your appointment has been cancelled successfully.");
    }

    // Shared helper for all appointment-related notifications. The email
    // send never throws (TrySendEmailAsync) and its real result is
    // returned to the caller, so a delivery failure is logged and reported
    // honestly without ever breaking the underlying appointment operation,
    // which has already been committed by the time this runs.
    private async Task<EmailSendResult> SendAppointmentEmailAsync(
        Appointment appointment,
        string subject,
        string introMessage)
    {
        var patient = await _unitOfWork
            .Repository<Patient>()
            .GetByIdAsync(appointment.PatientID);

        if (patient == null)
            return EmailSendResult.Fail("Patient record not found; could not determine the notification recipient.");

        var patientUser = await _unitOfWork
            .Repository<User>()
            .GetByIdAsync(patient.UserID);

        if (patientUser == null)
            return EmailSendResult.Fail("Patient's user account not found; could not determine the notification recipient.");

        var doctor = await _unitOfWork
            .Repository<Doctor>()
            .FirstOrDefaultAsync(
                d => d.DoctorID == appointment.DoctorID,
                d => d.User,
                d => d.User.Department);

        var doctorName = doctor?.User != null
            ? $"Dr. {doctor.User.FirstName} {doctor.User.LastName}"
            : "N/A";

        var departmentName = doctor?.User?.Department?.DepartmentName ?? string.Empty;

        var patientName = $"{patientUser.FirstName} {patientUser.LastName}".Trim();

        var body = EmailTemplates.AppointmentNotification(
            patientName,
            doctorName,
            departmentName,
            appointment.AppointmentDate,
            appointment.AppointmentTime,
            appointment.Status,
            subject,
            introMessage);

        var result = await _emailService.TrySendEmailAsync(patientUser.Email, subject, body);

        if (!result.Success)
        {
            _logger.LogError(
                "Failed to send appointment email for Appointment {AppointmentId}: {Error}",
                appointment.AppointmentID,
                result.ErrorMessage);
        }

        return result;
    }
}
