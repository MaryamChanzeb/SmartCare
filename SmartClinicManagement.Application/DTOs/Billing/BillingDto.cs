namespace SmartClinicManagement.Application.DTOs.Billing;

public class BillingDto
{
    public int BillID { get; set; }

    public int AppointmentID { get; set; }

    public decimal Amount { get; set; }

    public decimal Discount { get; set; }

    public decimal Tax { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    // Enriched for patient/receptionist billing views — resolved from the
    // linked Appointment/Payment records rather than stored redundantly.
    public int? PatientID { get; set; }

    public int? DoctorID { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? TransactionReference { get; set; }
}
