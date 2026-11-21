using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.DTOs.Patients;
using SmartClinicManagement.Application.Interfaces;
using System.Security.Claims;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // GET: api/Patients/me
    // Resolves the caller's OWN Patient profile from their JWT UserID —
    // the correct UserID → Patient.UserID relationship — instead of the
    // frontend having to fetch every patient and search for a match.
    // Auto-creates a minimal Patient row if one doesn't exist yet, which
    // also repairs any already-orphaned Patient-role account (e.g. one
    // created directly via the generic Users screen before the fix in
    // UserService.EnsureRoleProfileAsync) the first time they log in.
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "Unable to identify the logged-in user." });
        }

        try
        {
            var patient = await _patientService.GetOrCreateMyProfileAsync(userId);
            return Ok(patient);
        }
        catch (Exception ex)
        {
            // Only realistically hit if the User row itself is missing —
            // a clear message either way, never a raw 500.
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var patients = await _patientService.GetAllAsync();
        return Ok(patients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);

        if (patient == null)
            return NotFound();

        return Ok(patient);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePatientDto dto)
    {
        await _patientService.CreateAsync(dto);

        return Ok(new
        {
            Message = "Patient created successfully."
        });
    }

    // POST: api/Patients/register
    // Combined flow: creates the User account and Patient profile together
    // in one operation, with an optional assigned doctor. This is the one
    // proper patient registration flow — the receptionist never has to
    // pick an existing user from a dropdown.
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterPatientDto dto)
    {
        PatientRegistrationResult result;

        try
        {
            result = await _patientService.RegisterPatientAsync(dto);
        }
        catch (DuplicateEmailException ex)
        {
            // Clean, expected validation failure — never a raw exception.
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            // Other known business-rule failures from RegisterPatientAsync
            // (missing "Patient" role, doctor not found, etc.) — surfaced
            // as a clean 400 with the real reason instead of a raw 500.
            return BadRequest(new { Message = ex.Message });
        }

        // The patient record itself is always created here — only the
        // email outcome varies, and we report it honestly.
        if (!result.Email.Success)
        {
            return Ok(new
            {
                PatientID = result.PatientId,
                UserID = result.UserId,
                Message = $"Patient registered successfully, but the welcome email could not be sent: {result.Email.ErrorMessage}",
                EmailSent = false
            });
        }

        return Ok(new
        {
            PatientID = result.PatientId,
            UserID = result.UserId,
            Message = "Patient registered successfully and the welcome email was sent.",
            EmailSent = true
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdatePatientDto dto)
    {
        await _patientService.UpdateAsync(id, dto);

        return Ok(new
        {
            Message = "Patient updated successfully."
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _patientService.DeleteAsync(id);

        return Ok(new
        {
            Message = "Patient deleted successfully."
        });
    }
}
