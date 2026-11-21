namespace SmartClinicManagement.Application.DTOs.Users;

public class UpdateUserDto:CreateUserDto
{

    public string Status { get; set; } = string.Empty;

   
}