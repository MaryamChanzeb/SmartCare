using SmartClinicManagement.Application.DTOs.RoleActivities;

namespace SmartClinicManagement.Application.Interfaces;

public interface IRoleActivityService
{
    // Assign Activity to Role
    Task<RoleActivityDto> AssignActivityAsync(AssignRoleActivityDto dto);

    // Remove Assigned Activity
    Task<bool> RemoveActivityAsync(int roleActivityId);

    // Get All Activities of a Role
    Task<List<RoleActivitiesByRoleDto>> GetActivitiesByRoleAsync(int roleId);

    Task<List<RoleActivityDto>> GetAllAsync();
}