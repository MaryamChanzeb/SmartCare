using SmartClinicManagement.Application.DTOs.Roles;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using SmartClinicManagement.Infrastructure.UnitOfWork;

namespace SmartClinicManagement.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<List<RoleDto>> GetAllAsync()
    {
        var roles = await _unitOfWork
    .Repository<Role>()
    .GetAllAsync();

        return roles.Select(x => new RoleDto
        {
            RoleID = x.RoleID,
            RoleName = x.RoleName,
            Description = x.Description
        }).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await _unitOfWork
    .Repository<Role>()
    .GetByIdAsync(id);

        if (role == null)
            return null;

        return new RoleDto
        {
            RoleID = role.RoleID,
            RoleName = role.RoleName,
            Description = role.Description
        };
    }

    public async Task CreateAsync(CreateRoleDto dto)
    {
        var role = new Role
        {
            RoleName = dto.RoleName,
            Description = dto.Description
        };

        await _unitOfWork
    .Repository<Role>()
    .AddAsync(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, UpdateRoleDto dto)
    {
        var role = await _unitOfWork
    .Repository<Role>()
    .GetByIdAsync(id);

        if (role == null)
            return;

        role.RoleName = dto.RoleName;
        role.Description = dto.Description;
     

        await _unitOfWork
    .Repository<Role>()
    .UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var role = await _unitOfWork
    .Repository<Role>()
    .GetByIdAsync(id);

        if (role == null)
            return;

        role.IsDeleted = true;
        role.UpdatedOn = DateTime.UtcNow;


        await _unitOfWork
    .Repository<Role>()
    .UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();
    }
}