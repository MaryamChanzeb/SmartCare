using SmartClinicManagement.Application.DTOs.Departments;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;
using SmartClinicManagement.Infrastructure.UnitOfWork;

namespace SmartClinicManagement.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;


    public DepartmentService( IUnitOfWork unitOfWork)
    {
    
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await _unitOfWork
    .Repository<Department>().GetAllAsync();

        return departments.Select(d => new DepartmentDto
        {
            DepartmentID = d.DepartmentID,
            DepartmentName = d.DepartmentName,
            Description = d. Description
        }).ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department = await _unitOfWork
    .Repository<Department>().GetByIdAsync(id);

        if (department == null)
            return null;

        return new DepartmentDto
        {
            DepartmentID = department.DepartmentID,
            DepartmentName = department.DepartmentName,
            Description = department. Description
        };
    }

    public async Task CreateAsync(CreateDepartmentDto dto)
    {
        var department = new Department
        {
            DepartmentName = dto.DepartmentName,
            Description = dto. Description

        };

        await _unitOfWork
    .Repository<Department>().AddAsync(department);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var department = await _unitOfWork
    .Repository<Department>().GetByIdAsync(id);

        if (department == null)
            return;

        department.DepartmentName = dto.DepartmentName;
        department. Description = dto. Description;

        await _unitOfWork
    .Repository<Department>().UpdateAsync(department);
        await _unitOfWork.SaveChangesAsync();
    }

   
    public async Task DeleteAsync(int id)
    {
        var department = await _unitOfWork
    .Repository<Department>().GetByIdAsync(id);

        if (department == null)
            return;

        department.IsDeleted = true;
        department.UpdatedOn = DateTime.UtcNow;

        await _unitOfWork
    .Repository<Department>().UpdateAsync(department);
        await _unitOfWork.SaveChangesAsync();
    }
}