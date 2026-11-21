using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.DTOs.Patients;

namespace SmartClinicManagement.Application.Interfaces;

public interface IPatientService
{
    Task<List<PatientDto>> GetAllAsync();

    Task<PatientDto?> GetByIdAsync(int id);

    // Resolves the Patient profile linked to a given UserID (the correct
    // UserID → Patient.UserID relationship), auto-creating a minimal one
    // if it's missing so an already-orphaned Patient-role account self-
    // heals on first use instead of permanently showing "not linked".
    Task<PatientDto> GetOrCreateMyProfileAsync(int userId);

    Task CreateAsync(CreatePatientDto dto);

    // Combined registration: creates the User account and Patient profile
    // together in one operation, so callers never need to pick an existing
    // user from a dropdown. Returns the real welcome-email delivery
    // outcome so it can be reported accurately.
    Task<PatientRegistrationResult> RegisterPatientAsync(RegisterPatientDto dto);

    Task UpdateAsync(int id, UpdatePatientDto dto);

    Task DeleteAsync(int id);
}
