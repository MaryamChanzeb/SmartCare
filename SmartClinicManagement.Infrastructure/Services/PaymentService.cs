using SmartClinicManagement.Application.DTOs.Payments;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public PaymentService(
     IUnitOfWork unitOfWork,
     IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
    {
        var payments = await _unitOfWork
            .Repository<Payment>()
            .GetAllAsync(x => !x.IsDeleted);

        return payments.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetByBillAsync(int billId)
    {
        var payments = await _unitOfWork
            .Repository<Payment>()
            .GetAllAsync(x => x.BillID == billId && !x.IsDeleted);

        return payments.Select(MapToDto);
    }

    public async Task<PaymentDto?> GetByIdAsync(int id)
    {
        var payment = await _unitOfWork
            .Repository<Payment>()
            .FirstOrDefaultAsync(x => x.PaymentID == id && !x.IsDeleted);

        if (payment == null)
            return null;

        return MapToDto(payment);
    }

    public async Task<PaymentDto> ReceivePaymentAsync(ReceivePaymentDto dto)
    {
        var bill = await _unitOfWork
            .Repository<Billing>()
            .FirstOrDefaultAsync(x => x.BillID == dto.BillID && !x.IsDeleted);

        if (bill == null)
            throw new Exception("Bill not found.");

        var payment = new Payment
        {
            BillID = dto.BillID,
            Amount = dto.Amount,
            TransactionReference = dto.TransactionReference,
            PaymentDate = DateTime.Now,
            Status = dto.Status
        };

        await _unitOfWork.Repository<Payment>().AddAsync(payment);

        if (dto.Status == "Success")
        {
            bill.PaymentStatus = "Paid";
            bill.UpdatedOn = DateTime.Now;

            await _unitOfWork.Repository<Billing>().UpdateAsync(bill);
        }

        await _unitOfWork.SaveChangesAsync();


if (bill != null)
{
    var appointment = await _unitOfWork
        .Repository<Appointment>()
        .GetByIdAsync(bill.AppointmentID);

    if (appointment != null)
    {
        var patient = await _unitOfWork
            .Repository<Patient>()
            .GetByIdAsync(appointment.PatientID);

        if (patient != null)
        {
            var user = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(patient.UserID);

            if (user != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Payment Successful",
                    $@"
                    <h2>Payment Received</h2>

                    <p>Dear {user.FirstName},</p>

                    <p>Your payment has been received successfully.</p>

                    <table border='1' cellpadding='8'>
                        <tr>
                            <td><b>Amount Paid</b></td>
                            <td>{dto.Amount}</td>
                        </tr>

                        <tr>
                            <td><b>Status</b></td>
                            <td>Success</td>
                        </tr>

                        <tr>
                            <td><b>Reference</b></td>
                            <td>{dto.TransactionReference}</td>
                        </tr>
                    </table>

                    <br/>

                    Thank you.");
            }
        }
    }
}

        return MapToDto(payment);
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            PaymentID = payment.PaymentID,
            BillID = payment.BillID,
            TransactionReference = payment.TransactionReference,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status
        };
    }
}