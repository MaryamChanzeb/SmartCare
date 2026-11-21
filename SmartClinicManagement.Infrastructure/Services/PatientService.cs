using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.DTOs.Patients;
using SmartClinicManagement.Application.DTOs.Users;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using System.Linq.Expressions;

namespace SmartClinicManagement.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public PatientService(
        IUnitOfWork unitOfWork,
        IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
    }

    public async Task<List<PatientDto>> GetAllAsync()
    {
        var (patients, _) = await _unitOfWork
     .Repository<Patient>()
     .GetAllOrderedAsync(
         predicate: p => !p.IsDeleted,
         orderByDescending: p => p.CreatedOn,
         pageNumber: null,
         pageSize: null,
         includes: new Expression<Func<Patient, object>>[] { p => p.User });



        return patients.Select(p => new PatientDto
        {
            PatientID = p.PatientID,
            UserID = p.UserID,
            BloodGroup = p.BloodGroup,
            Height = p.Height,
            Weight = p.Weight,
            EmergencyContactName = p.EmergencyContactName,
            EmergencyContactPhone = p.EmergencyContactPhone,
            Allergies = p.Allergies,
            MedicalHistory = p.MedicalHistory,
            Status = p.Status,
            PrimaryDoctorID = p.PrimaryDoctorID
        }).ToList();
    }

    public async Task<PatientDto?> GetByIdAsync(int id)
    {
        var patient = await _unitOfWork
            .Repository<Patient>()
            .FirstOrDefaultAsync(
                x => x.PatientID == id && !x.IsDeleted,
                x => x.User);

        if (patient == null)
            return null;

        return new PatientDto
        {
            PatientID = patient.PatientID,
            UserID = patient.UserID,
            BloodGroup = patient.BloodGroup,
            Height = patient.Height,
            Weight = patient.Weight,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            Allergies = patient.Allergies,
            MedicalHistory = patient.MedicalHistory,
            Status = patient.Status,
            PrimaryDoctorID = patient.PrimaryDoctorID
        };
    }
    public async Task<PatientDto> GetOrCreateMyProfileAsync(int userId)
    {
        var existing = await _unitOfWork
            .Repository<Patient>()
            .FirstOrDefaultAsync(x => x.UserID == userId && !x.IsDeleted, x => x.User);

        if (existing != null)
        {
            return MapToDto(existing);
        }

        var user = await _unitOfWork
            .Repository<User>()
            .FirstOrDefaultAsync(x => x.UserID == userId && !x.IsDeleted);

        if (user == null)
        {
            throw new Exception("User account not found.");
        }

        var patient = new Patient
        {
            UserID = userId,
            Status = "Active",
            CreatedOn = DateTime.Now,
            IsDeleted = false
        };

        await _unitOfWork.Repository<Patient>().AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(patient);
    }

    private static PatientDto MapToDto(Patient patient)
    {
        return new PatientDto
        {
            PatientID = patient.PatientID,
            UserID = patient.UserID,
            BloodGroup = patient.BloodGroup,
            Height = patient.Height,
            Weight = patient.Weight,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            Allergies = patient.Allergies,
            MedicalHistory = patient.MedicalHistory,
            Status = patient.Status,
            PrimaryDoctorID = patient.PrimaryDoctorID
        };
    }

    public async Task CreateAsync(CreatePatientDto dto)
    {
        var userExists = await _unitOfWork
            .Repository<User>()
            .AnyAsync(x => x.UserID == dto.UserID);

        if (!userExists)
            throw new Exception("User does not exist.");

        var patientExists = await _unitOfWork
            .Repository<Patient>()
            .AnyAsync(x => x.UserID == dto.UserID && !x.IsDeleted);

        if (patientExists)
            throw new Exception("Patient already exists for this user.");

        var patient = new Patient
        {
            UserID = dto.UserID,
            BloodGroup = dto.BloodGroup,
            Height = dto.Height,
            Weight = dto.Weight,
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone,
            Allergies = dto.Allergies,
            MedicalHistory = dto.MedicalHistory,
            Status = dto.Status,
            PrimaryDoctorID = dto.PrimaryDoctorID
        };

        await _unitOfWork.Repository<Patient>().AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PatientRegistrationResult> RegisterPatientAsync(RegisterPatientDto dto)
    {
        // "Patient" role is looked up by name rather than hardcoding an ID,
        // since roles are managed dynamically via the Roles screen.
        var patientRole = await _unitOfWork
            .Repository<Role>()
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == "patient");

        if (patientRole == null)
            throw new Exception(
                "No \"Patient\" role exists yet. Create one on the Roles screen first, then try registering again.");

        if (dto.DoctorID.HasValue)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.DoctorID == dto.DoctorID.Value && !d.IsDeleted);

            if (!doctorExists)
                throw new Exception("Selected doctor not found.");
        }

        var userDto = new CreateUserDto
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            DepartmentID = null,
            RoleID = patientRole.RoleID
        };

        // Creates the User account and attempts the welcome/credentials
        // email via the existing UserService flow. The real email outcome
        // (not an assumption) is carried back to the caller below.
        var accountResult = await _userService.RegisterAccountAsync(userDto);

        var patient = new Patient
        {
            UserID = accountResult.UserId,
            BloodGroup = dto.BloodGroup,
            Height = dto.Height,
            Weight = dto.Weight,
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone,
            Allergies = dto.Allergies,
            MedicalHistory = dto.MedicalHistory,
            Status = dto.Status,
            PrimaryDoctorID = dto.DoctorID
        };

        await _unitOfWork.Repository<Patient>().AddAsync(patient);
        await _unitOfWork.SaveChangesAsync();

        return new PatientRegistrationResult
        {
            PatientId = patient.PatientID,
            UserId = accountResult.UserId,
            Email = accountResult.Email
        };
    }

    public async Task UpdateAsync(int id, UpdatePatientDto dto)
    {
        var patient = await _unitOfWork
            .Repository<Patient>()
            .GetByIdAsync(id);

        if (patient == null || patient.IsDeleted)
            return;

        patient.BloodGroup = dto.BloodGroup;
        patient.Height = dto.Height;
        patient.Weight = dto.Weight;
        patient.EmergencyContactName = dto.EmergencyContactName;
        patient.EmergencyContactPhone = dto.EmergencyContactPhone;
        patient.Allergies = dto.Allergies;
        patient.MedicalHistory = dto.MedicalHistory;
        patient.Status = dto.Status;
        patient.PrimaryDoctorID = dto.PrimaryDoctorID;
        patient.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork.Repository<Patient>().UpdateAsync(patient);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var patient = await _unitOfWork
            .Repository<Patient>()
            .GetByIdAsync(id);

        if (patient == null || patient.IsDeleted)
            return;

        patient.IsDeleted = true;
        patient.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork.Repository<Patient>().UpdateAsync(patient);
        await _unitOfWork.SaveChangesAsync();
    }
}
