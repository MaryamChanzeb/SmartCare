using SmartClinicManagement.Application.DTOs.Permissions;

namespace SmartClinicManagement.Application.Interfaces;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetPermissionsByRoleAsync(int roleId);

    Task<PermissionDto> UpdatePermissionAsync(UpdatePermissionDto dto);
}