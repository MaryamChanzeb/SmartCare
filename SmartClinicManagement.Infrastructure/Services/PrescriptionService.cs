using SmartClinicManagement.Application.DTOs.Prescriptions;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Infrastructure.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PrescriptionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AddPrescriptionAsync(CreatePrescriptionDto dto)
    {
        var medicalRecordRepository = _unitOfWork.Repository<MedicalRecord>();
        var prescriptionRepository = _unitOfWork.Repository<Prescription>();

        var recordExists = await medicalRecordRepository.AnyAsync(x =>
            x.RecordID == dto.RecordID &&
            !x.IsDeleted);

        if (!recordExists)
            throw new Exception("Medical Record not found.");

        var prescription = new Prescription
        {
            RecordID = dto.RecordID,
            MedicineName = dto.MedicineName,
            Dosage = dto.Dosage,
            Frequency = dto.Frequency,
            Duration = dto.Duration,
            Instructions = dto.Instructions,
            CreatedOn = DateTime.UtcNow,
            IsDeleted = false
        };

        await prescriptionRepository.AddAsync(prescription);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdatePrescriptionAsync(
        int id,
        UpdatePrescriptionDto dto)
    {
        var repository = _unitOfWork.Repository<Prescription>();

        var prescription = await repository.GetByIdAsync(id);

        if (prescription == null || prescription.IsDeleted)
            throw new Exception("Prescription not found.");

        prescription.RecordID = dto.RecordID;
        prescription.MedicineName = dto.MedicineName;
        prescription.Dosage = dto.Dosage;
        prescription.Frequency = dto.Frequency;
        prescription.Duration = dto.Duration;
        prescription.Instructions = dto.Instructions;
        prescription.UpdatedOn = DateTime.UtcNow;

        await repository.UpdateAsync(prescription);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PrescriptionDto?> GetPrescriptionByIdAsync(int id)
    {
        var repository = _unitOfWork.Repository<Prescription>();

        var prescription = await repository.FirstOrDefaultAsync(x =>
            x.PrescriptionID == id &&
            !x.IsDeleted);

        if (prescription == null)
            return null;

        return new PrescriptionDto
        {
            PrescriptionID = prescription.PrescriptionID,
            RecordID = prescription.RecordID,
            MedicineName = prescription.MedicineName,
            Dosage = prescription.Dosage,
            Frequency = prescription.Frequency,
            Duration = prescription.Duration,
            Instructions = prescription.Instructions
        };
    }

    public async Task<List<PrescriptionDto>> GetMedicalRecordPrescriptionsAsync(int recordId)
    {
        var repository = _unitOfWork.Repository<Prescription>();

        var prescriptions = await repository.GetAllAsync(x =>
            x.RecordID == recordId &&
            !x.IsDeleted);

        return prescriptions.Select(x => new PrescriptionDto
        {
            PrescriptionID = x.PrescriptionID,
            RecordID = x.RecordID,
            MedicineName = x.MedicineName,
            Dosage = x.Dosage,
            Frequency = x.Frequency,
            Duration = x.Duration,
            Instructions = x.Instructions
        }).ToList();
    }
}