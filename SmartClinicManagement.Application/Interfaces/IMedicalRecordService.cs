using SmartClinicManagement.Application.DTOs.MedicalRecords;

namespace SmartClinicManagement.Application.Interfaces;

public interface IMedicalRecordService
{
    Task CreateMedicalRecordAsync(CreateMedicalRecordDto dto);

    Task UpdateMedicalRecordAsync(int id, UpdateMedicalRecordDto dto);

    Task<MedicalRecordDto?> GetRecordByIdAsync(int id);

    Task<List<MedicalRecordDto>> GetPatientHistoryAsync(int patientId);

    Task<List<MedicalRecordDto>> GetDoctorRecordsAsync(int doctorId);

    // All non-deleted records — a medical record only ever exists after a
    // checkup/appointment is completed, so this is effectively "all
    // completed checkup records." Used by Receptionist/Admin.
    Task<List<MedicalRecordDto>> GetAllRecordsAsync();
}