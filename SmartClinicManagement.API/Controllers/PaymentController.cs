using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClinicManagement.Application.DTOs.Payments;
using SmartClinicManagement.Application.Interfaces;

namespace SmartClinicManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _paymentService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _paymentService.GetByIdAsync(id);

        if (payment == null)
            return NotFound();

        return Ok(payment);
    }

    [HttpGet("bill/{billId}")]
    public async Task<IActionResult> GetByBill(int billId)
    {
        return Ok(await _paymentService.GetByBillAsync(billId));
    }

    [HttpPost]
    public async Task<IActionResult> ReceivePayment(ReceivePaymentDto dto)
    {
        var payment = await _paymentService.ReceivePaymentAsync(dto);

        return Ok(payment);
    }
}