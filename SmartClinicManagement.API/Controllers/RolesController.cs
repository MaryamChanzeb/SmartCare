using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Roles;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    // GET: api/Roles
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }

    // GET: api/Roles/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _roleService.GetByIdAsync(id);

        if (role == null)
            return NotFound();

        return Ok(role);
    }

    // POST: api/Roles
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleDto dto)
    {
        await _roleService.CreateAsync(dto);

        return Ok(new
        {
            Message = "Role created successfully."
        });
    }

    // PUT: api/Roles/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateRoleDto dto)
    {
        await _roleService.UpdateAsync(id, dto);

        return Ok(new
        {
            Message = "Role updated successfully."
        });
    }

    // DELETE: api/Roles/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roleService.DeleteAsync(id);

        return Ok(new
        {
            Message = "Role deleted successfully."
        });
    }
}