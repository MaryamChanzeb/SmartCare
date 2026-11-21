using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Activities;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    // GET: api/activities
    [HttpGet]
    public async Task<IActionResult> GetAllActivities()
    {
        var activities = await _activityService.GetAllAsync();

        return Ok(activities);
    }

    // GET: api/activities/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetActivityById(int id)
    {
        var activity = await _activityService.GetByIdAsync(id);

        if (activity == null)
            return NotFound(new
            {
                Message = "Activity not found."
            });

        return Ok(activity);
    }

    // POST: api/activities
    [HttpPost]
    public async Task<IActionResult> CreateActivity(CreateActivityDto dto)
    {
        var activity = await _activityService.CreateAsync(dto);

        return Ok(new
        {
            Message = "Activity created successfully.",
            Data = activity
        });
    }

    // PUT: api/activities/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateActivity(int id, UpdateActivityDto dto)
    {
        var activity = await _activityService.UpdateAsync(id, dto);

        return Ok(new
        {
            Message = "Activity updated successfully.",
            Data = activity
        });
    }

    // DELETE: api/activities/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        var deleted = await _activityService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                Message = "Activity not found."
            });
        }

        return Ok(new
        {
            Message = "Activity deleted successfully."
        });
    }
}