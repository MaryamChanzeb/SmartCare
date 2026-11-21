using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.RoleActivities;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleActivitiesController : ControllerBase
{
    private readonly IRoleActivityService _roleActivityService;

    public RoleActivitiesController(IRoleActivityService roleActivityService)
    {
        _roleActivityService = roleActivityService;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleActivityService.GetAllAsync();

        return Ok(result);
    }

  

    [HttpGet("role/{roleId}")]
    public async Task<IActionResult> GetActivitiesByRole(int roleId)
    {
        var result =
            await _roleActivityService.GetActivitiesByRoleAsync(roleId);

        return Ok(result);
    }


    [HttpPost("assign")]
    public async Task<IActionResult> AssignActivity(AssignRoleActivityDto dto)
    {
        try
        {
            var result = await _roleActivityService.AssignActivityAsync(dto);

            return Ok(new
            {
                Success = true,
                Message = "Activity assigned successfully.",
                Data = result
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



    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _roleActivityService.RemoveActivityAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Assignment not found."
            });
        }

        return Ok(new
        {
            Success = true,
            Message = "Activity removed successfully."
        });
    }
}