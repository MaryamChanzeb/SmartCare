using SmartClinicManagement.Application.DTOs.Roles;

namespace SmartClinicManagement.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync();

    Task<RoleDto?> GetByIdAsync(int id);

    Task CreateAsync(CreateRoleDto dto);

    Task UpdateAsync(int id, UpdateRoleDto dto);

    Task DeleteAsync(int id);
}