using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Departments;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _departmentService.GetAllAsync();

        return Ok(departments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _departmentService.GetByIdAsync(id);

        if (department == null)
            return NotFound();

        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        await _departmentService.CreateAsync(dto);

        return Ok(new
        {
            Message = "Department created successfully."
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateDepartmentDto dto)
    {
        await _departmentService.UpdateAsync(id, dto);

        return Ok(new
        {
            Message = "Department updated successfully."
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _departmentService.DeleteAsync(id);

        return Ok(new
        {
            Message = "Department deleted successfully."
        });
    }
}