namespace SmartClinicManagement.Application.DTOs.Authentication;

public class LoginResponseDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public int UserID { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}