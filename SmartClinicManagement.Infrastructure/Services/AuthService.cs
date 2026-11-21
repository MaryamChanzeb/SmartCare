using Microsoft.Extensions.Configuration;
using SmartClinicManagement.Application.DTOs.Authentication;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using SmartClinicManagement.Infrastructure.Helpers;

namespace SmartClinicManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    // Existing dependencies — unchanged, still used by Login/Logout/ChangePassword.
    private readonly IGenericRepository<User> _userRepository;
    private readonly IPasswordRepository _passwordRepository;
    private readonly IJwtService _jwtService;

    // Added for Register/ForgotPassword/ResetPassword only.
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    // Roles the public Register endpoint is allowed to hand out.
    // Admin/SuperAdmin (and anything else) must be created by an
    // existing Admin through the Users management screen.
    private static readonly string[] AllowedRegistrationRoles =
        { "Patient", "Doctor", "Receptionist" };

    public AuthService(
      IGenericRepository<User> userRepository,
      IPasswordRepository passwordRepository,
      IJwtService jwtService,
      IUnitOfWork unitOfWork,
      IEmailService emailService,
      IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordRepository = passwordRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task LogoutAsync(int userId)
    {
        var passwordDetails =
            await _passwordRepository.GetCurrentPasswordAsync(userId);

        if (passwordDetails == null)
            return;

        passwordDetails.LastLogoutTime = DateTime.Now;

        passwordDetails.SessionID = null;

        await _passwordRepository.UpdateAsync(passwordDetails);
    }
    public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var passwordDetails =
            await _passwordRepository.GetCurrentPasswordAsync(dto.UserID);

        if (passwordDetails == null)
            return false;

        var currentHash =
            PasswordHelper.HashPassword(dto.CurrentPassword);

        if (passwordDetails.PasswordHash != currentHash)
            return false;

        if (dto.NewPassword != dto.ConfirmPassword)
            return false;

        passwordDetails.PasswordHash =
            PasswordHelper.HashPassword(dto.NewPassword);

        passwordDetails.UpdatedOn = DateTime.Now;

        await _passwordRepository.UpdateAsync(passwordDetails);

        return true;
    }
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.FirstOrDefaultAsync(
     x => x.Email == dto.Email && !x.IsDeleted,
     x => x.Role);

        if (user == null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid Email or Password."
            };
        }

        var passwordDetails =
            await _passwordRepository.GetCurrentPasswordAsync(user.UserID);

        if (passwordDetails == null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid Email or Password."
            };
        }

        var hashedPassword = PasswordHelper.HashPassword(dto.Password);

        if (hashedPassword != passwordDetails.PasswordHash)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid Email or Password."
            };
        }

        passwordDetails.LastLoginTime = DateTime.Now;
        passwordDetails.SessionID = Guid.NewGuid().ToString();

        await _passwordRepository.UpdateAsync(passwordDetails);
        var token = _jwtService.GenerateToken(user);


        return new LoginResponseDto
        {
            Success = true,
            Message = "Login Successful.",

            Token = token,

            ExpiresAt = DateTime.UtcNow.AddMinutes(60),

            UserID = user.UserID,

            FullName = $"{user.FirstName} {user.LastName}",

            Role = user.Role?.RoleName ?? ""
        };
    }



    // Register


    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
        {
            return new RegisterResponseDto
            {
                Success = false,
                Message = "Password and confirmation do not match."
            };
        }

        var requestedRole = dto.RoleName?.Trim() ?? string.Empty;

        var isAllowedRole = AllowedRegistrationRoles.Any(
            r => string.Equals(r, requestedRole, StringComparison.OrdinalIgnoreCase));

        if (!isAllowedRole)
        {
            return new RegisterResponseDto
            {
                Success = false,
                Message = "Please select a valid role (Patient, Doctor, or Receptionist)."
            };
        }

        var emailInUse = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Email == dto.Email && !u.IsDeleted);

        if (emailInUse)
        {
            return new RegisterResponseDto
            {
                Success = false,
                Message = "An account with this email already exists."
            };
        }

        // RoleID is never hardcoded — it is looked up from the Roles table
        // by name, the same table the Admin's Role Management screen edits.
        var role = await _unitOfWork.Repository<Role>().FirstOrDefaultAsync(
            r => r.RoleName == requestedRole && !r.IsDeleted);

        if (role == null)
        {
            return new RegisterResponseDto
            {
                Success = false,
                Message = $"The '{requestedRole}' role is not configured. Please contact the clinic administrator."
            };
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            RoleID = role.RoleID,
            Status = "Active",
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
            CreatedOn = DateTime.Now,
            IsDeleted = false
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Create the password record — without this, LoginAsync's
        // GetCurrentPasswordAsync lookup would find nothing and the
        // new account could never log in.
        var passwordDetails = new PasswordDetails
        {
            UserID = user.UserID,
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            IsCurrent = true,
            LastLoginTime = DateTime.Now,
            CreatedOn = DateTime.Now,
            IsDeleted = false
        };

        await _unitOfWork.Repository<PasswordDetails>().AddAsync(passwordDetails);

        // Create the role-specific record so the rest of the app (which
        // matches Doctor/Patient rows to a logged-in user by UserID) works
        // immediately after registration.
        if (string.Equals(role.RoleName, "Doctor", StringComparison.OrdinalIgnoreCase))
        {
            var doctor = new Doctor
            {
                UserID = user.UserID,
                LicenseNumber = string.IsNullOrWhiteSpace(dto.LicenseNumber)
                    ? "Pending Verification"
                    : dto.LicenseNumber!,
                Specialization = string.IsNullOrWhiteSpace(dto.Specialization)
                    ? "General Practice"
                    : dto.Specialization!,
                ExperienceYears = 0,
                ConsultationFee = 0,
                Status = "Active",
                CreatedOn = DateTime.Now,
                IsDeleted = false
            };

            await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
        }
        else if (string.Equals(role.RoleName, "Patient", StringComparison.OrdinalIgnoreCase))
        {
            var patient = new Patient
            {
                UserID = user.UserID,
                Status = "Active",
                CreatedOn = DateTime.Now,
                IsDeleted = false
            };

            await _unitOfWork.Repository<Patient>().AddAsync(patient);
        }
        // Receptionist has no dedicated profile table today — the User
        // row plus RoleID is all the rest of the app expects for it.

        await _unitOfWork.SaveChangesAsync();

        // The account itself is fully created at this point. Sending the
        // welcome email was previously never attempted here at all
        // (IEmailService was injected but unused) — now it's actually
        // sent, and its real outcome is reported instead of assumed.
        var clientBaseUrl = _configuration["AppSettings:ClientBaseUrl"] ?? "http://localhost:4200";
        var loginUrl = $"{clientBaseUrl}/login";
        var fullName = $"{user.FirstName} {user.LastName}".Trim();

        var welcomeBody = EmailTemplates.SelfRegistrationWelcome(
            fullName,
            user.Email,
            role.RoleName,
            loginUrl);

        var emailResult = await _emailService.TrySendEmailAsync(
            user.Email,
            "Welcome to Smart Clinic",
            welcomeBody);

        return new RegisterResponseDto
        {
            Success = true,
            Message = emailResult.Success
                ? "Account created successfully. A confirmation email has been sent. You can now log in."
                : "Account created successfully, but the confirmation email could not be sent. You can still log in.",
            EmailSent = emailResult.Success,
            EmailError = emailResult.ErrorMessage
        };
    }



    // Forgot / Reset Password

    public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        // Always return the same generic message whether or not the email
        // exists, so this endpoint can't be used to enumerate accounts.
        var genericResponse = new AuthResponseDto
        {
            Success = true,
            Message = "If an account exists for that email, a password reset link has been sent."
        };

        var user = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(
            u => u.Email == dto.Email && !u.IsDeleted);

        if (user == null)
        {
            return genericResponse;
        }

        var passwordDetails =
            await _passwordRepository.GetCurrentPasswordAsync(user.UserID);

        if (passwordDetails == null)
        {
            return genericResponse;
        }

        passwordDetails.ResetToken = Guid.NewGuid().ToString("N");
        passwordDetails.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);

        await _passwordRepository.UpdateAsync(passwordDetails);

        var clientBaseUrl = _configuration["AppSettings:ClientBaseUrl"] ?? "http://localhost:4200";
        var resetLink = $"{clientBaseUrl}/reset-password?token={passwordDetails.ResetToken}";

        var body = $@"
            <p>Hello {user.FirstName},</p>
            <p>We received a request to reset your Smart Clinic Management password.</p>
            <p><a href=""{resetLink}"">Click here to reset your password</a></p>
            <p>This link expires in 30 minutes. If you didn't request this, you can ignore this email.</p>";

        // Never throws — the endpoint must keep returning its generic
        // response either way, but the failure is still logged for
        // diagnosis instead of being silently lost.
        var emailResult = await _emailService.TrySendEmailAsync(
            user.Email,
            "Reset your Smart Clinic Management password",
            body);

        if (!emailResult.Success)
        {
            Console.WriteLine($"[ForgotPassword] Email send failed. Reason: {emailResult.ErrorMessage}");
            Console.WriteLine($"[ForgotPassword] Reset link for {user.Email}: {resetLink}");
        }

        return genericResponse;
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "New password and confirmation do not match."
            };
        }

        var passwordDetails = await _unitOfWork.Repository<PasswordDetails>().FirstOrDefaultAsync(
            p => p.ResetToken == dto.Token &&
                 p.IsCurrent &&
                 !p.IsDeleted);

        if (passwordDetails == null ||
            passwordDetails.ResetTokenExpiry == null ||
            passwordDetails.ResetTokenExpiry < DateTime.UtcNow)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "This reset link is invalid or has expired. Please request a new one."
            };
        }

        passwordDetails.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
        passwordDetails.ResetToken = null;
        passwordDetails.ResetTokenExpiry = null;
        passwordDetails.UpdatedOn = DateTime.Now;

        await _unitOfWork.Repository<PasswordDetails>().UpdateAsync(passwordDetails);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto
        {
            Success = true,
            Message = "Your password has been reset successfully. You can now log in."
        };
    }
}
