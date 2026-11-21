using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.MedicalRecords;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MedicalRecordsController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    // GET: api/MedicalRecords
    // All completed checkup records — used by Receptionist/Admin, who
    // don't search by a single patient/doctor the way Doctor/Patient do.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var records = await _medicalRecordService.GetAllRecordsAsync();

        return Ok(records);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMedicalRecordDto dto)
    {
        await _medicalRecordService.CreateMedicalRecordAsync(dto);

        return Ok(new
        {
            Message = "Medical record created successfully."
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateMedicalRecordDto dto)
    {
        await _medicalRecordService.UpdateMedicalRecordAsync(id, dto);

        return Ok(new
        {
            Message = "Medical record updated successfully."
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var record = await _medicalRecordService.GetRecordByIdAsync(id);

        if (record == null)
            return NotFound();

        return Ok(record);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetPatientHistory(int patientId)
    {
        var records = await _medicalRecordService.GetPatientHistoryAsync(patientId);

        return Ok(records);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetDoctorRecords(int doctorId)
    {
        var records = await _medicalRecordService.GetDoctorRecordsAsync(doctorId);

        return Ok(records);
    }
}