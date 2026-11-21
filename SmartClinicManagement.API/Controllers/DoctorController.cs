using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Doctors;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);

        if (doctor == null)
            return NotFound();

        return Ok(doctor);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDoctorDto dto)
    {
        await _doctorService.CreateAsync(dto);

        return Ok(new
        {
            Message = "Doctor created successfully."
        });
    }

    
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDoctorDto dto)
    {
        await _doctorService.RegisterDoctorAsync(dto);

        return Ok(new
        {
            Message = "Doctor registered successfully."
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateDoctorDto dto)
    {
        await _doctorService.UpdateAsync(id, dto);

        return Ok(new
        {
            Message = "Doctor updated successfully."
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _doctorService.DeleteAsync(id);

        return Ok(new
        {
            Message = "Doctor deleted successfully."
        });
    }
}