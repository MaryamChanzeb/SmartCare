using SmartClinicManagement.Application.DTOs.Authentication;

namespace SmartClinicManagement.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task LogoutAsync(int userId);
    Task<bool> ChangePasswordAsync(ChangePasswordDto dto);

    Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto dto);
}
