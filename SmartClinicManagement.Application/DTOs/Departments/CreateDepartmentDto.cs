using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Departments;

public class CreateDepartmentDto
{
    [Required]
    public string DepartmentName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}