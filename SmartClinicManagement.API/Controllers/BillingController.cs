using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Billing;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);

        var userIdClaim =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Ok(Array.Empty<BillingDto>());


        // PATIENT → only own bills
        if (string.Equals(
                role,
                "Patient",
                StringComparison.OrdinalIgnoreCase))
        {
            var myBills =
                await _billingService.GetForPatientUserAsync(userId);

            return Ok(myBills);
        }


        // DOCTOR → only bills of their own patients
        if (string.Equals(
                role,
                "Doctor",
                StringComparison.OrdinalIgnoreCase))
        {
            var myBills =
                await _billingService.GetForDoctorUserAsync(userId);

            return Ok(myBills);
        }


        // ADMIN / RECEPTIONIST / ACCOUNTANT → existing behavior
        var result = await _billingService.GetAllAsync();

        return Ok(result);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);

        var userIdClaim =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();


        // Doctor → only his own patients' bills
        if (string.Equals(
                role,
                "Doctor",
                StringComparison.OrdinalIgnoreCase))
        {
            var doctorBill =
                await _billingService.GetForDoctorUserByIdAsync(
                    id,
                    userId);

            if (doctorBill == null)
                return NotFound();

            return Ok(doctorBill);
        }


        // Existing behavior for other roles
        var result = await _billingService.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    // Creating/editing bills is a front-desk/accounts task. Admin is
    // view-only for billing, and a Patient must never write billing data.
    [HttpPost]
    [Authorize(Roles = "Receptionist,Accountant")]
    public async Task<IActionResult> Create(CreateBillingDto dto)
    {
        var result = await _billingService.CreateAsync(dto);

        return CreatedAtAction(nameof(GetById),
            new { id = result.BillID }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Receptionist,Accountant")]
    public async Task<IActionResult> Update(int id, UpdateBillingDto dto)
    {
        var result = await _billingService.UpdateAsync(id, dto);

        if (!result)
            return NotFound(new { message = "Bill not found." });

        return Ok(new { message = "Bill updated successfully." });
    }

   
    [HttpPut("{id}/pay")]
    [Authorize(Roles = "Receptionist")]
    public async Task<IActionResult> MarkAsPaid(
        int id,
        MarkBillPaidDto dto)
    {
        var result = await _billingService.MarkAsPaidAsync(id, dto);

        if (!result)
            return NotFound(new { message = "Bill not found." });

       
        return Ok(new { message = "Payment received successfully." });
    }
}
