using SmartClinicManagement.Application.DTOs.Departments;

namespace SmartClinicManagement.Application.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();

    Task<DepartmentDto?> GetByIdAsync(int id);

    Task CreateAsync(CreateDepartmentDto dto);

    Task UpdateAsync(int id, UpdateDepartmentDto dto);

    Task DeleteAsync(int id);
}