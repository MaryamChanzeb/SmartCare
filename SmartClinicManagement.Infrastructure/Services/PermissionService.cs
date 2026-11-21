using Microsoft.EntityFrameworkCore;
using SmartClinicManagement.Application.DTOs.Permissions;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using SmartClinicManagement.Persistence.Context;

namespace SmartClinicManagement.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }



    public async Task<List<PermissionDto>> GetPermissionsByRoleAsync(int roleId)
    {
        var permissions = await _context.Permissions
    .Include(x => x.RoleActivity)
        .ThenInclude(x => x.Role)
    .Include(x => x.RoleActivity)
        .ThenInclude(x => x.Activity)
    .Where(x => x.RoleActivity != null &&
                x.RoleActivity.RoleID == roleId &&
                !x.IsDeleted)
    .ToListAsync();

    

        return permissions.Select(x => new PermissionDto
        {
            PermissionID = x.PermissionID,

            RoleActivityID = x.RoleActivityID,

            RoleID = x.RoleActivity!.RoleID,

            RoleName = x.RoleActivity.Role!.RoleName,

            ActivityID = x.RoleActivity.ActivityID,

            ActivityName = x.RoleActivity.Activity!.ActivityName,

            CanCreate = x.CanCreate,

            CanRead = x.CanRead,

            CanUpdate = x.CanUpdate,

            CanDelete = x.CanDelete

        }).ToList();
    }

   

    public async Task<PermissionDto> UpdatePermissionAsync(UpdatePermissionDto dto)
    {
        var repository = _unitOfWork.Repository<Permission>();

        var permission = await _context.Permissions
    .Include(x => x.RoleActivity)
        .ThenInclude(x => x.Role)
    .Include(x => x.RoleActivity)
        .ThenInclude(x => x.Activity)
    .FirstOrDefaultAsync(x =>
        x.PermissionID == dto.PermissionID &&
        !x.IsDeleted);

        if (permission == null)
            throw new KeyNotFoundException("Permission not found.");

        permission.CanCreate = dto.CanCreate;
        permission.CanRead = dto.CanRead;
        permission.CanUpdate = dto.CanUpdate;
        permission.CanDelete = dto.CanDelete;

      

        await _unitOfWork.SaveChangesAsync();

        return new PermissionDto
        {
            PermissionID = permission.PermissionID,

            RoleActivityID = permission.RoleActivityID,

            RoleID = permission.RoleActivity!.RoleID,

            RoleName = permission.RoleActivity.Role!.RoleName,

            ActivityID = permission.RoleActivity.ActivityID,

            ActivityName = permission.RoleActivity.Activity!.ActivityName,

            CanCreate = permission.CanCreate,

            CanRead = permission.CanRead,

            CanUpdate = permission.CanUpdate,

            CanDelete = permission.CanDelete
        };
    }
}