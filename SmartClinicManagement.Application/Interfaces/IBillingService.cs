using SmartClinicManagement.Application.DTOs.Billing;

namespace SmartClinicManagement.Application.Interfaces;

public interface IBillingService
{
    Task<IEnumerable<BillingDto>> GetAllAsync();

    // Bills belonging only to the patient linked to the given User account.
    // Used to enforce that a Patient can only ever see their own bills.
    Task<IEnumerable<BillingDto>> GetForPatientUserAsync(int userId);
    Task<IEnumerable<BillingDto>> GetForDoctorUserAsync(int userId);

    Task<BillingDto?> GetForDoctorUserByIdAsync(int id, int userId);
   

    Task<BillingDto?> GetByIdAsync(int id);

    Task<BillingDto> CreateAsync(CreateBillingDto dto);

    Task<bool> UpdateAsync(int id, UpdateBillingDto dto);

    Task<bool> MarkAsPaidAsync(int id, MarkBillPaidDto dto);
}
