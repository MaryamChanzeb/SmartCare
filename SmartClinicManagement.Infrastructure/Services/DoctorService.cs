using SmartClinicManagement.Application.DTOs.Doctors;
using SmartClinicManagement.Application.DTOs.Users;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using System.Linq.Expressions;

namespace SmartClinicManagement.Infrastructure.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public DoctorService(
        IUnitOfWork unitOfWork,
        IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
    }

    public async Task<List<DoctorDto>> GetAllAsync()
    {
        // Sorted at the database level (ORDER BY CreatedOn DESC) so newly
        // added doctors show up first without pulling the whole table
        // into memory just to sort/take the newest ones.
        var (doctors, _) = await _unitOfWork
            .Repository<Doctor>()
            .GetAllOrderedAsync(
                predicate: x => !x.IsDeleted,
                orderByDescending: x => x.CreatedOn,
                pageNumber: null,
                pageSize: null,
                includes: new Expression<Func<Doctor, object>>[] { x => x.User });

        return doctors.Select(d => new DoctorDto
        {
            DoctorID = d.DoctorID,
            UserID = d.UserID,
            LicenseNumber = d.LicenseNumber,
            Specialization = d.Specialization,
            Qualification = d.Qualification,
            ExperienceYears = d.ExperienceYears,
            ConsultationFee = d.ConsultationFee,
            RoomNumber = d.RoomNumber,
            JoiningDate = d.JoiningDate,
            Status = d.Status
        }).ToList();
    }

    public async Task<DoctorDto?> GetByIdAsync(int id)
    {
        var doctor = await _unitOfWork
            .Repository<Doctor>()
            .FirstOrDefaultAsync(x => x.DoctorID == id && !x.IsDeleted, x => x.User);

        if (doctor == null)
            return null;

        return new DoctorDto
        {
            DoctorID = doctor.DoctorID,
            UserID = doctor.UserID,
            LicenseNumber = doctor.LicenseNumber,
            Specialization = doctor.Specialization,
            Qualification = doctor.Qualification,
            ExperienceYears = doctor.ExperienceYears,
            ConsultationFee = doctor.ConsultationFee,
            RoomNumber = doctor.RoomNumber,
            JoiningDate = doctor.JoiningDate,
            Status = doctor.Status
        };
    }

    public async Task CreateAsync(CreateDoctorDto dto)
    {
        var userExists = await _unitOfWork
            .Repository<User>()
            .AnyAsync(x => x.UserID == dto.UserID);

        if (!userExists)
            throw new Exception("User does not exist.");

        var doctorExists = await _unitOfWork
            .Repository<Doctor>()
            .AnyAsync(x => x.UserID == dto.UserID && !x.IsDeleted);

        if (doctorExists)
            throw new Exception("Doctor already exists for this user.");

        var licenseExists = await _unitOfWork
            .Repository<Doctor>()
            .AnyAsync(x => x.LicenseNumber == dto.LicenseNumber && !x.IsDeleted);

        if (licenseExists)
            throw new Exception("License number already exists.");

        var doctor = new Doctor
        {
            UserID = dto.UserID,
            LicenseNumber = dto.LicenseNumber,
            Specialization = dto.Specialization,
            Qualification = dto.Qualification,
            ExperienceYears = dto.ExperienceYears,
            ConsultationFee = dto.ConsultationFee,
            RoomNumber = dto.RoomNumber,
            JoiningDate = dto.JoiningDate,
            Status = dto.Status
        };

        await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RegisterDoctorAsync(RegisterDoctorDto dto)
    {
        // "Doctor" role is looked up by name rather than hardcoding an ID,
        // since roles are managed dynamically via the Roles screen.
        var doctorRole = await _unitOfWork
            .Repository<Role>()
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == "doctor");

        if (doctorRole == null)
            throw new Exception(
                "No \"Doctor\" role exists yet. Create one on the Roles screen first, then try registering again.");

        var licenseExists = await _unitOfWork
            .Repository<Doctor>()
            .AnyAsync(x => x.LicenseNumber == dto.LicenseNumber && !x.IsDeleted);

        if (licenseExists)
            throw new Exception("License number already exists.");

        var userDto = new CreateUserDto
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            DepartmentID = dto.DepartmentID,
            RoleID = doctorRole.RoleID
        };

        // Creates the User account (and emails the generated credentials
        // via the existing UserService flow).
        var accountResult = await _userService.RegisterAccountAsync(userDto);




        var doctor = new Doctor
        {
            UserID = accountResult.UserId,
            LicenseNumber = dto.LicenseNumber,
            Specialization = dto.Specialization,
            Qualification = dto.Qualification,
            ExperienceYears = dto.ExperienceYears,
            ConsultationFee = dto.ConsultationFee,
            RoomNumber = dto.RoomNumber,
            JoiningDate = dto.JoiningDate,
            Status = dto.Status
        };

        await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, UpdateDoctorDto dto)
    {
        var doctor = await _unitOfWork
            .Repository<Doctor>()
            .GetByIdAsync(id);

        if (doctor == null || doctor.IsDeleted)
            return;

        doctor.LicenseNumber = dto.LicenseNumber;
        doctor.Specialization = dto.Specialization;
        doctor.Qualification = dto.Qualification;
        doctor.ExperienceYears = dto.ExperienceYears;
        doctor.ConsultationFee = dto.ConsultationFee;
        doctor.RoomNumber = dto.RoomNumber;
        doctor.JoiningDate = dto.JoiningDate;
        doctor.Status = dto.Status;
        doctor.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork.Repository<Doctor>().UpdateAsync(doctor);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await _unitOfWork
            .Repository<Doctor>()
            .GetByIdAsync(id);

        if (doctor == null || doctor.IsDeleted)
            return;

        doctor.IsDeleted = true;
        doctor.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork.Repository<Doctor>().UpdateAsync(doctor);
        await _unitOfWork.SaveChangesAsync();
    }
}