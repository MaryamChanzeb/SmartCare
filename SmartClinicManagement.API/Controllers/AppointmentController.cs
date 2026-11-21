using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Appointments;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // GET: api/Appointments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();

        return Ok(appointments);
    }

    // GET: api/Appointments/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);

        if (appointment == null)
            return NotFound(new
            {
                Message = "Appointment not found."
            });

        return Ok(appointment);
    }
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var emailResult = await _appointmentService.ConfirmAppointmentAsync(id);

        if (!emailResult.Success)
        {
            return Ok(new
            {
                message = $"Appointment confirmed successfully, but the notification email could not be sent: {emailResult.ErrorMessage}",
                EmailSent = false
            });
        }

        return Ok(new
        {
            message = "Appointment confirmed successfully and the notification email was sent.",
            EmailSent = true
        });
    }

    // POST: api/Appointments
    [HttpPost]
    public async Task<IActionResult> BookAppointment(BookAppointmentDto dto)
    {
        var emailResult = await _appointmentService.BookAppointmentAsync(dto);

        // The appointment itself is always booked at this point — only the
        // email outcome varies, and we report it honestly.
        if (!emailResult.Success)
        {
            return Ok(new
            {
                Message = $"Appointment booked successfully, but the confirmation email could not be sent: {emailResult.ErrorMessage}",
                EmailSent = false
            });
        }

        return Ok(new
        {
            Message = "Appointment booked successfully and the confirmation email was sent.",
            EmailSent = true
        });
    }

    // PUT: api/Appointments/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, UpdateAppointmentDto dto)
    {
        var emailResult = await _appointmentService.UpdateAppointmentAsync(id, dto);

        // Null means this particular update (no reschedule / no fresh
        // confirmation) never triggers a notification email at all.
        if (emailResult == null)
        {
            return Ok(new
            {
                Message = "Appointment updated successfully."
            });
        }

        if (!emailResult.Success)
        {
            return Ok(new
            {
                Message = $"Appointment updated successfully, but the notification email could not be sent: {emailResult.ErrorMessage}",
                EmailSent = false
            });
        }

        return Ok(new
        {
            Message = "Appointment updated successfully and the notification email was sent.",
            EmailSent = true
        });
    }

    // PUT: api/Appointments/5/cancel
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var emailResult = await _appointmentService.CancelAppointmentAsync(id);

        if (!emailResult.Success)
        {
            return Ok(new
            {
                Message = $"Appointment cancelled successfully, but the notification email could not be sent: {emailResult.ErrorMessage}",
                EmailSent = false
            });
        }

        return Ok(new
        {
            Message = "Appointment cancelled successfully and the notification email was sent.",
            EmailSent = true
        });
    }
}
