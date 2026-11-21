using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Payments;

public class ReceivePaymentDto
{
    [Required]
    public int BillID { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public string? TransactionReference { get; set; }

    public string Status { get; set; } = "Success";
}