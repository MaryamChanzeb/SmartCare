using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Billing;

public class CreateBillingDto
{
    [Required]
    public int AppointmentID { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public decimal Discount { get; set; }

    public decimal Tax { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "Cash";
}