using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartClinicManagement.Application.Common;
using SmartClinicManagement.Application.DTOs.Users;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using SmartClinicManagement.Infrastructure.Configuration;
using SmartClinicManagement.Infrastructure.Helpers;
using System.Linq.Expressions;

namespace SmartClinicManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordRepository _passwordRepository;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordRepository passwordRepository,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordRepository = passwordRepository;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var (users, _) = await _unitOfWork
                    .Repository<User>()
                    .GetAllOrderedAsync(
                        predicate: null,
                        orderByDescending: x => x.CreatedOn,
                        pageNumber: null,
                        pageSize: null,
                        includes: new Expression<Func<User, object>>[] { x => x.Role, x => x.Department });


        return users.Select(u => new UserDto
        {
            UserID = u.UserID,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Phone = u.Phone,
            Gender = u.Gender,
            DateOfBirth = u.DateOfBirth,
            Status = u.Status,
            DepartmentName = u.Department?.DepartmentName,
            RoleName = u.Role?.RoleName
        }).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _unitOfWork
    .Repository<User>().FirstOrDefaultAsync(
    x => x.UserID == id,
    x => x.Role,
    x => x.Department);

        if (user == null)
            return null;

        return new UserDto
        {
            UserID = user.UserID,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Status = user.Status,
            DepartmentName = user.Department?.DepartmentName,
            RoleName = user.Role?.RoleName
        };
    }

    public async Task<AccountCreationResult> CreateAsync(CreateUserDto dto)
    {

        var result = await RegisterAccountAsync(dto);

        await EnsureRoleProfileAsync(result.UserId, dto.RoleID);

        return result;
    }


    // Creates a minimal Patient/Doctor profile row for a freshly-created
    // User if the assigned role needs one and doesn't already have it.
    // Safe to call unconditionally: it's a no-op for roles that don't need
    // a profile row, and for a UserID that already has one.
    private async Task EnsureRoleProfileAsync(int userId, int? roleId)
    {
        if (!roleId.HasValue) return;

        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(roleId.Value);

        if (role == null) return;

        if (string.Equals(role.RoleName, "Patient", StringComparison.OrdinalIgnoreCase))
        {
            var alreadyLinked = await _unitOfWork
                .Repository<Patient>()
                .AnyAsync(p => p.UserID == userId && !p.IsDeleted);

            if (alreadyLinked) return;

            await _unitOfWork.Repository<Patient>().AddAsync(new Patient
            {
                UserID = userId,
                Status = "Active",
                CreatedOn = DateTime.Now,
                IsDeleted = false
            });

            await _unitOfWork.SaveChangesAsync();
        }
        else if (string.Equals(role.RoleName, "Doctor", StringComparison.OrdinalIgnoreCase))
        {
            var alreadyLinked = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(d => d.UserID == userId && !d.IsDeleted);

            if (alreadyLinked) return;

            await _unitOfWork.Repository<Doctor>().AddAsync(new Doctor
            {
                UserID = userId,
                LicenseNumber = "Pending Verification",
                Specialization = "General Practice",
                ExperienceYears = 0,
                ConsultationFee = 0,
                Status = "Active",
                CreatedOn = DateTime.Now,
                IsDeleted = false
            });

            await _unitOfWork.SaveChangesAsync();
        }
    }


    public async Task<AccountCreationResult> RegisterAccountAsync(CreateUserDto dto)
    {
        var emailInUse = await _unitOfWork
    .Repository<User>()
    .AnyAsync(x => x.Email == dto.Email && !x.IsDeleted);

        if (emailInUse)
        {
            throw new DuplicateEmailException(
                "An account with this email already exists.");
        }


        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            DepartmentID = dto.DepartmentID,
            RoleID = dto.RoleID,
            Status = "Active",
            CreatedOn = DateTime.Now,
            IsDeleted = false
        };

        await _unitOfWork
    .Repository<User>().AddAsync(user);

        await _unitOfWork.SaveChangesAsync();

        // Generate login credentials for the new account. The plain-text
        // password is only ever used to send the notification email below;
        // only its hash is persisted (existing hashing/auth flow is reused).
        var temporaryPassword = PasswordHelper.GenerateTemporaryPassword();

        var passwordDetails = new PasswordDetails
        {
            UserID = user.UserID,
            PasswordHash = PasswordHelper.HashPassword(temporaryPassword),
            IsCurrent = true,
            CreatedOn = DateTime.Now
        };

        await _passwordRepository.CreateAsync(passwordDetails);

        // The account is already persisted at this point — email delivery
        // is attempted separately and its real outcome (not an assumption)
        // is returned to the caller so it never breaks account creation.
        var emailResult = await SendRegistrationEmailAsync(user, dto.RoleID, temporaryPassword);

        return new AccountCreationResult
        {
            UserId = user.UserID,
            Email = emailResult
        };
    }

    private async Task<EmailSendResult> SendRegistrationEmailAsync(
        User user,
        int? roleId,
        string temporaryPassword)
    {
        string roleName = "User";

        if (roleId.HasValue)
        {
            var role = await _unitOfWork
                .Repository<Role>()
                .GetByIdAsync(roleId.Value);

            if (role != null)
                roleName = role.RoleName;
        }

        var fullName = $"{user.FirstName} {user.LastName}".Trim();

        var isPatient = string.Equals(
            roleName, "Patient", StringComparison.OrdinalIgnoreCase);

        var subject = isPatient
            ? "Welcome to Smart Clinic"
            : "Your Smart Clinic Account Credentials";

        var body = isPatient
            ? EmailTemplates.PatientWelcome(
                fullName,
                user.Email,
                temporaryPassword,
                roleName,
                _emailSettings.LoginUrl)
            : EmailTemplates.UserCredentials(
                fullName,
                user.Email,
                temporaryPassword,
                roleName,
                _emailSettings.LoginUrl);

        // TrySendEmailAsync never throws — it returns the real outcome
        // instead, so registration itself is never broken by an SMTP
        // failure while the caller still learns the actual result.
        var result = await _emailService.TrySendEmailAsync(user.Email, subject, body);

        if (!result.Success)
        {
            _logger.LogError(
                "Failed to send registration email to user {UserId} ({Email}): {Error}",
                user.UserID,
                user.Email,
                result.ErrorMessage);
        }

        return result;
    }

    public async Task UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _unitOfWork
    .Repository<User>().GetByIdAsync(id);

        if (user == null)
            throw new Exception("User not found.");
        Console.WriteLine("=========================");
        Console.WriteLine($"Status = '{dto.Status}'");
        Console.WriteLine("=========================");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.Phone = dto.Phone;
        user.Gender = dto.Gender;
        user.DateOfBirth = dto.DateOfBirth;
        user.DepartmentID = dto.DepartmentID;
        user.RoleID = dto.RoleID;
        user.Status = dto.Status;
        user.UpdatedOn = DateTime.Now;

        await _unitOfWork
    .Repository<User>().UpdateAsync(user);

        await _unitOfWork.SaveChangesAsync();
    }


    public async Task DeleteAsync(int id)
    {
        var user = await _unitOfWork
    .Repository<User>().GetByIdAsync(id);

        if (user == null)
            return;

        user.IsDeleted = true;
        user.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork
    .Repository<User>().UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }
}
