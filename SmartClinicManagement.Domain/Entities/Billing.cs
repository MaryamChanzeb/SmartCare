using SmartClinicManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Domain.Entities;

public class Billing : BaseEntity
{
    [Key]
    public int BillID { get; set; }

    public int AppointmentID { get; set; }

    public decimal Amount { get; set; }

    public decimal Discount { get; set; }

    public decimal Tax { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } = "Cash";

    public string PaymentStatus { get; set; } = "Pending";
}