using SmartClinicManagement.Application.DTOs.Billing;
using SmartClinicManagement.Application.DTOs.Payments;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Infrastructure.Services;

public class BillingService : IBillingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;

    public BillingService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IPaymentService paymentService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _paymentService = paymentService;
    }

    public async Task<IEnumerable<BillingDto>> GetAllAsync()
    {
        var bills = await _unitOfWork
            .Repository<Billing>()
            .GetAllAsync(x => !x.IsDeleted);

        var result = new List<BillingDto>();

        foreach (var bill in bills)
        {
            result.Add(await MapToDtoAsync(bill));
        }

        return result;
    }

    public async Task<IEnumerable<BillingDto>> GetForPatientUserAsync(int userId)
    {
        // Resolve which Patient (and therefore which appointments/bills)
        // belongs to this logged-in User, then filter strictly to those —
        // a Patient must never see another patient's billing/payment data.
        var patient = await _unitOfWork
            .Repository<Patient>()
            .FirstOrDefaultAsync(x => x.UserID == userId && !x.IsDeleted);

        if (patient == null)
            return Enumerable.Empty<BillingDto>();

        var myAppointmentIds = (await _unitOfWork
            .Repository<Appointment>()
            .GetAllAsync(x => x.PatientID == patient.PatientID && !x.IsDeleted))
            .Select(a => a.AppointmentID)
            .ToHashSet();

        var bills = await _unitOfWork
            .Repository<Billing>()
            .GetAllAsync(x => !x.IsDeleted);

        var myBills = bills.Where(b => myAppointmentIds.Contains(b.AppointmentID));

        var result = new List<BillingDto>();

        foreach (var bill in myBills)
        {
            result.Add(await MapToDtoAsync(bill));
        }

        return result;
    }
    public async Task<IEnumerable<BillingDto>> GetForDoctorUserAsync(int userId)
    {
        // Find the Doctor linked to the logged-in User
        var doctor = await _unitOfWork
            .Repository<Doctor>()
            .FirstOrDefaultAsync(x => x.UserID == userId && !x.IsDeleted);

        if (doctor == null)
            return Enumerable.Empty<BillingDto>();

        // Get only appointments belonging to this Doctor
        var myAppointmentIds = (await _unitOfWork
            .Repository<Appointment>()
            .GetAllAsync(x =>
                x.DoctorID == doctor.DoctorID &&
                !x.IsDeleted))
            .Select(a => a.AppointmentID)
            .ToHashSet();

        // Get bills
        var bills = await _unitOfWork
            .Repository<Billing>()
            .GetAllAsync(x => !x.IsDeleted);

        // Keep only bills for this Doctor's appointments
        var myBills = bills
            .Where(b => myAppointmentIds.Contains(b.AppointmentID));

        var result = new List<BillingDto>();

        foreach (var bill in myBills)
        {
            result.Add(await MapToDtoAsync(bill));
        }

        return result;
    }
    public async Task<BillingDto?> GetForDoctorUserByIdAsync(
    int id,
    int userId)
    {
        var doctor = await _unitOfWork
            .Repository<Doctor>()
            .FirstOrDefaultAsync(x =>
                x.UserID == userId &&
                !x.IsDeleted);

        if (doctor == null)
            return null;

        var bill = await _unitOfWork
            .Repository<Billing>()
            .FirstOrDefaultAsync(x =>
                x.BillID == id &&
                !x.IsDeleted);

        if (bill == null)
            return null;

        var appointment = await _unitOfWork
            .Repository<Appointment>()
            .FirstOrDefaultAsync(x =>
                x.AppointmentID == bill.AppointmentID &&
                !x.IsDeleted);

        if (appointment == null ||
            appointment.DoctorID != doctor.DoctorID)
        {
            return null;
        }

        return await MapToDtoAsync(bill);
    }
    public async Task<BillingDto?> GetByIdAsync(int id)
    {
        var bill = await _unitOfWork
            .Repository<Billing>()
            .FirstOrDefaultAsync(x => x.BillID == id && !x.IsDeleted);

        if (bill == null)
            return null;

        return await MapToDtoAsync(bill);
    }

    public async Task<BillingDto> CreateAsync(CreateBillingDto dto)
    {
        var appointmentExists = await _unitOfWork
            .Repository<Appointment>()
            .AnyAsync(x => x.AppointmentID == dto.AppointmentID && !x.IsDeleted);

        if (!appointmentExists)
            throw new Exception("Appointment not found.");

        var billAlreadyExists = await _unitOfWork
            .Repository<Billing>()
            .AnyAsync(x => x.AppointmentID == dto.AppointmentID && !x.IsDeleted);

        if (billAlreadyExists)
            throw new Exception("A bill already exists for this appointment.");

        var bill = new Billing
        {
            AppointmentID = dto.AppointmentID,
            Amount = dto.Amount,
            Discount = dto.Discount,
            Tax = dto.Tax,
            TotalAmount = dto.Amount - dto.Discount + dto.Tax,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = "Pending"
        };

        await _unitOfWork.Repository<Billing>().AddAsync(bill);

        await _unitOfWork.SaveChangesAsync();
        // Get Appointment
        var appointment = await _unitOfWork
            .Repository<Appointment>()
            .GetByIdAsync(dto.AppointmentID);

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
                    try
                    {
                        await _emailService.SendEmailAsync(
                            user.Email,
                            "Hospital Bill Generated",
                            $@"
                <h2>Smart Clinic Billing</h2>

                <p>Dear {user.FirstName},</p>

                <p>Your bill has been generated successfully.</p>

                <table border='1' cellpadding='8'>
                    <tr>
                        <td><b>Amount</b></td>
                        <td>{bill.Amount}</td>
                    </tr>

                    <tr>
                        <td><b>Discount</b></td>
                        <td>{bill.Discount}</td>
                    </tr>

                    <tr>
                        <td><b>Tax</b></td>
                        <td>{bill.Tax}</td>
                    </tr>

                    <tr>
                        <td><b>Total</b></td>
                        <td>{bill.TotalAmount}</td>
                    </tr>

                    <tr>
                        <td><b>Status</b></td>
                        <td>{bill.PaymentStatus}</td>
                    </tr>
                </table>

                <br/>

                Thank you.");
                    }
                    catch
                    {
                        // Email failure must never fail bill creation.
                    }
                }
            }
        }

        return await GetByIdAsync(bill.BillID) ?? throw new Exception("Bill creation failed.");
    }

    public async Task<bool> UpdateAsync(int id, UpdateBillingDto dto)
    {
        var bill = await _unitOfWork
            .Repository<Billing>()
            .FirstOrDefaultAsync(x => x.BillID == id && !x.IsDeleted);

        if (bill == null)
            return false;

        bill.Amount = dto.Amount;
        bill.Discount = dto.Discount;
        bill.Tax = dto.Tax;
        bill.PaymentMethod = dto.PaymentMethod;
        bill.TotalAmount = dto.Amount - dto.Discount + dto.Tax;
        bill.UpdatedOn = DateTime.Now;

        await _unitOfWork.Repository<Billing>().UpdateAsync(bill);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAsPaidAsync(int id, MarkBillPaidDto dto)
    {
        var bill = await _unitOfWork
            .Repository<Billing>()
            .FirstOrDefaultAsync(x => x.BillID == id && !x.IsDeleted);

        if (bill == null)
            return false;

        bill.PaymentMethod = dto.PaymentMethod;
        bill.UpdatedOn = DateTime.Now;

        await _unitOfWork.Repository<Billing>().UpdateAsync(bill);

        await _unitOfWork.SaveChangesAsync();

        // Creates the actual Payment record (so it shows up under Admin →
        // Payments) and marks the bill Paid — reuses the existing payment
        // pipeline instead of duplicating that logic here.
        await _paymentService.ReceivePaymentAsync(new ReceivePaymentDto
        {
            BillID = bill.BillID,
            Amount = bill.TotalAmount,
            TransactionReference = null,
            Status = "Success"
        });

        return true;
    }

    private async Task<BillingDto> MapToDtoAsync(Billing bill)
    {
        var dto = new BillingDto
        {
            BillID = bill.BillID,
            AppointmentID = bill.AppointmentID,
            Amount = bill.Amount,
            Discount = bill.Discount,
            Tax = bill.Tax,
            TotalAmount = bill.TotalAmount,
            PaymentMethod = bill.PaymentMethod,
            PaymentStatus = bill.PaymentStatus
        };

        var appointment = await _unitOfWork
            .Repository<Appointment>()
            .GetByIdAsync(bill.AppointmentID);

        if (appointment != null)
        {
            dto.PatientID = appointment.PatientID;
            dto.DoctorID = appointment.DoctorID;
        }

        var payments = await _unitOfWork
            .Repository<Payment>()
            .GetAllAsync(x => x.BillID == bill.BillID && !x.IsDeleted);

        var latestPayment = payments
            .OrderByDescending(p => p.PaymentDate)
            .FirstOrDefault();

        if (latestPayment != null)
        {
            dto.PaymentDate = latestPayment.PaymentDate;
            dto.TransactionReference = latestPayment.TransactionReference;
        }

        return dto;
    }
}
