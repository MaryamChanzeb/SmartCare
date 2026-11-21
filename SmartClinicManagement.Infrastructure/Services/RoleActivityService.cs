using SmartClinicManagement.Application.DTOs.RoleActivities;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Application.Services;

public class RoleActivityService : IRoleActivityService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleActivityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Assign Activity To Role
    
    public async Task<RoleActivityDto> AssignActivityAsync(AssignRoleActivityDto dto)
    {
        var roleRepository = _unitOfWork.Repository<Role>();
        var activityRepository = _unitOfWork.Repository<Activity>();
        var roleActivityRepository = _unitOfWork.Repository<RoleActivities>();

        // Check Role Exists
        var role = await roleRepository.GetByIdAsync(dto.RoleID);

        if (role == null)
            throw new KeyNotFoundException("Role not found.");

        // Check Activity Exists
        var activity = await activityRepository.GetByIdAsync(dto.ActivityID);

        if (activity == null)
            throw new KeyNotFoundException("Activity not found.");

        // Check Duplicate Assignment
        bool alreadyAssigned = await roleActivityRepository.AnyAsync(x =>
            x.RoleID == dto.RoleID &&
            x.ActivityID == dto.ActivityID &&
            !x.IsDeleted);

        if (alreadyAssigned)
            throw new InvalidOperationException("Activity is already assigned to this role.");

        var entity = new RoleActivities
        {
            RoleID = dto.RoleID,
            ActivityID = dto.ActivityID,
            AssignedBy = dto.AssignedBy,
            AssignedDate = DateTime.UtcNow,
            Status = "Active"
        };

        await roleActivityRepository.AddAsync(entity);

        await _unitOfWork.SaveChangesAsync();

        return new RoleActivityDto
        {
            RoleActivityID = entity.RoleActivityID,
            RoleID = role.RoleID,
            RoleName = role.RoleName,
            ActivityID = activity.ActivityID,
            ActivityName = activity.ActivityName,
            AssignedBy = entity.AssignedBy,
            AssignedDate = entity.AssignedDate,
            Status = entity.Status
        };
    }

    // Remove Assignment

    public async Task<bool> RemoveActivityAsync(int roleActivityId)
    {
        var repo = _unitOfWork.Repository<RoleActivities>();

        var entity = await repo.GetByIdAsync(roleActivityId);

        if (entity == null)
            return false;

        if (entity.IsDeleted)
            return false;

        entity.IsDeleted = true;
        entity.Status = "Inactive";

        await repo.UpdateAsync(entity);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // Get Activities By Role

    public async Task<List<RoleActivitiesByRoleDto>> GetActivitiesByRoleAsync(int roleId)
    {
        var repository = _unitOfWork.Repository<RoleActivities>();

        var data = await repository.GetAllAsync(
            x => x.RoleID == roleId && !x.IsDeleted,
            x => x.Activity);

        return data.Select(x => new RoleActivitiesByRoleDto
        {
            ActivityID = x.ActivityID,
            ActivityName = x.Activity?.ActivityName ?? "",
            Description = x.Activity?.Description,
            MenuURL = x.Activity?.MenuURL,
            Status = x.Status
        }).ToList();
    }
    public async Task<List<RoleActivityDto>> GetAllAsync()
    {
        var repository = _unitOfWork.Repository<RoleActivities>();

        var data = await repository.GetAllAsync(
            x => !x.IsDeleted,
            x => x.Role,
            x => x.Activity);

        return data.Select(x => new RoleActivityDto
        {
            RoleActivityID = x.RoleActivityID,
            RoleID = x.RoleID,
            RoleName = x.Role?.RoleName ?? "",
            ActivityID = x.ActivityID,
            ActivityName = x.Activity?.ActivityName ?? "",
            AssignedBy = x.AssignedBy,
            AssignedDate = x.AssignedDate,
            Status = x.Status
        }).ToList();
    }
}