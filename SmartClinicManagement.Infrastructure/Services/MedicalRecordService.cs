using SmartClinicManagement.Application.DTOs.MedicalRecords;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Infrastructure.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IUnitOfWork _unitOfWork;

    public MedicalRecordService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateMedicalRecordAsync(CreateMedicalRecordDto dto)
    {
        var doctorRepository = _unitOfWork.Repository<Doctor>();
        var patientRepository = _unitOfWork.Repository<Patient>();
        var medicalRepository = _unitOfWork.Repository<MedicalRecord>();

        var doctorExists = await doctorRepository.AnyAsync(x =>
            x.DoctorID == dto.DoctorID && !x.IsDeleted);

        if (!doctorExists)
            throw new Exception("Doctor not found.");

        var patientExists = await patientRepository.AnyAsync(x =>
            x.PatientID == dto.PatientID && !x.IsDeleted);

        if (!patientExists)
            throw new Exception("Patient not found.");

        var medicalRecord = new MedicalRecord
        {
            PatientID = dto.PatientID,
            DoctorID = dto.DoctorID,
            Diagnosis = dto.Diagnosis,
            Symptoms = dto.Symptoms,
            Treatment = dto.Treatment,
            Notes = dto.Notes,
            VisitDate = dto.VisitDate,
            CreatedOn = DateTime.UtcNow,
            IsDeleted = false
        };

        await medicalRepository.AddAsync(medicalRecord);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateMedicalRecordAsync(int id, UpdateMedicalRecordDto dto)
    {
        var repository = _unitOfWork.Repository<MedicalRecord>();

        var record = await repository.GetByIdAsync(id);

        if (record == null || record.IsDeleted)
            throw new Exception("Medical record not found.");

        record.Diagnosis = dto.Diagnosis;
        record.Symptoms = dto.Symptoms;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.VisitDate = dto.VisitDate;
        record.UpdatedOn = DateTime.UtcNow;

        await repository.UpdateAsync(record);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<MedicalRecordDto?> GetRecordByIdAsync(int id)
    {
        var repository = _unitOfWork.Repository<MedicalRecord>();

        var record = await repository.FirstOrDefaultAsync(x =>
            x.RecordID == id && !x.IsDeleted);

        if (record == null)
            return null;

        return new MedicalRecordDto
        {
            RecordID = record.RecordID,
            PatientID = record.PatientID,
            DoctorID = record.DoctorID,
            Diagnosis = record.Diagnosis,
            Symptoms = record.Symptoms,
            Treatment = record.Treatment,
            Notes = record.Notes,
            VisitDate = record.VisitDate
        };
    }

    public async Task<List<MedicalRecordDto>> GetPatientHistoryAsync(int patientId)
    {
        var repository = _unitOfWork.Repository<MedicalRecord>();

        var records = await repository.GetAllAsync(x =>
            x.PatientID == patientId && !x.IsDeleted);

        return records
            .OrderByDescending(x => x.VisitDate)
            .Select(x => new MedicalRecordDto
            {
                RecordID = x.RecordID,
                PatientID = x.PatientID,
                DoctorID = x.DoctorID,
                Diagnosis = x.Diagnosis,
                Symptoms = x.Symptoms,
                Treatment = x.Treatment,
                Notes = x.Notes,
                VisitDate = x.VisitDate
            }).ToList();
    }

    public async Task<List<MedicalRecordDto>> GetDoctorRecordsAsync(int doctorId)
    {
        var repository = _unitOfWork.Repository<MedicalRecord>();

        var records = await repository.GetAllAsync(x =>
            x.DoctorID == doctorId && !x.IsDeleted);

        return records.Select(x => new MedicalRecordDto
        {
            RecordID = x.RecordID,
            PatientID = x.PatientID,
            DoctorID = x.DoctorID,
            Diagnosis = x.Diagnosis,
            Symptoms = x.Symptoms,
            Treatment = x.Treatment,
            Notes = x.Notes,
            VisitDate = x.VisitDate
        }).ToList();
    }

    public async Task<List<MedicalRecordDto>> GetAllRecordsAsync()
    {
        var repository = _unitOfWork.Repository<MedicalRecord>();

        var records = await repository.GetAllAsync(x => !x.IsDeleted);

        return records
            .OrderByDescending(x => x.VisitDate)
            .Select(x => new MedicalRecordDto
            {
                RecordID = x.RecordID,
                PatientID = x.PatientID,
                DoctorID = x.DoctorID,
                Diagnosis = x.Diagnosis,
                Symptoms = x.Symptoms,
                Treatment = x.Treatment,
                Notes = x.Notes,
                VisitDate = x.VisitDate
            }).ToList();
    }
}