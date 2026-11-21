namespace SmartClinicManagement.Application.DTOs.Authentication;

public class RegisterResponseDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    // Real outcome of the welcome email attempt — never assumed true just because Success is true
    public bool EmailSent { get; set; }

    public string? EmailError { get; set; }
}
