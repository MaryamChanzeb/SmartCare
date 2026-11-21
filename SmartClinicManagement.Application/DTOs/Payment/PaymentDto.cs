namespace SmartClinicManagement.Application.DTOs.Payments;

public class PaymentDto
{
    public int PaymentID { get; set; }

    public int BillID { get; set; }

    public string? TransactionReference { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = string.Empty;
}