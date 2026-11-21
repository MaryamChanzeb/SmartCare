using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Billing;

public class MarkBillPaidDto
{
    [Required]
    public string PaymentMethod { get; set; } = "Cash";
}