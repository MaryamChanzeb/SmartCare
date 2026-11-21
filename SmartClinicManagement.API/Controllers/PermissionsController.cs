using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Permissions;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    // ==================================================
    // GET: api/Permissions/role/1
    // ==================================================

    [HttpGet("role/{roleId}")]
    public async Task<IActionResult> GetPermissionsByRole(int roleId)
    {
        try
        {
            var permissions =
                await _permissionService.GetPermissionsByRoleAsync(roleId);

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // ==================================================
    // PUT: api/Permissions/update
    // ==================================================

    [HttpPut("update")]
    public async Task<IActionResult> UpdatePermission(
        UpdatePermissionDto dto)
    {
        try
        {
            var permission =
                await _permissionService.UpdatePermissionAsync(dto);

            return Ok(new
            {
                Success = true,
                Message = "Permission updated successfully.",
                Data = permission
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
}