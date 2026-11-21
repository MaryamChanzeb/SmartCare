using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Prescriptions;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionsController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreatePrescriptionDto dto)
    {
        await _prescriptionService.AddPrescriptionAsync(dto);

        return Ok(new
        {
            Message = "Prescription added successfully."
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdatePrescriptionDto dto)
    {
        await _prescriptionService.UpdatePrescriptionAsync(id, dto);

        return Ok(new
        {
            Message = "Prescription updated successfully."
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);

        if (prescription == null)
            return NotFound();

        return Ok(prescription);
    }

    [HttpGet("record/{recordId}")]
    public async Task<IActionResult> GetByMedicalRecord(int recordId)
    {
        var prescriptions = await _prescriptionService
            .GetMedicalRecordPrescriptionsAsync(recordId);

        return Ok(prescriptions);
    }
}