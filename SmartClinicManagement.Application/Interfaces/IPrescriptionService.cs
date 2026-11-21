using SmartClinicManagement.Application.DTOs.Prescriptions;

namespace SmartClinicManagement.Application.Interfaces;

public interface IPrescriptionService
{
    Task AddPrescriptionAsync(CreatePrescriptionDto dto);

    Task UpdatePrescriptionAsync(
        int id,
        UpdatePrescriptionDto dto);

    Task<PrescriptionDto?> GetPrescriptionByIdAsync(
        int id);

    Task<List<PrescriptionDto>> GetMedicalRecordPrescriptionsAsync(
        int recordId);
}