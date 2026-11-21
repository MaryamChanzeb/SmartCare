using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class Payment : BaseEntity
{
    public int PaymentID { get; set; }

    public int BillID { get; set; }

    public string? TransactionReference { get; set; }

    public string Status { get; set; } = "Pending";
    public decimal? Amount { get; set; }
    public DateTime? PaymentDate { get; set; }
   
}