using SmartClinicManagement.Application.DTOs.Dashboard;
using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardDto> GetDashboardAsync()
{
    return new DashboardDto
    {
        TotalUsers = await _unitOfWork
            .Repository<User>()
            .CountAsync(x => !x.IsDeleted),

        TotalDepartments = await _unitOfWork
            .Repository<Department>()
            .CountAsync(x => !x.IsDeleted),

        TotalRoles = await _unitOfWork
            .Repository<Role>()
            .CountAsync(x => !x.IsDeleted),

        TotalDoctors = await _unitOfWork
            .Repository<Doctor>()
            .CountAsync(x => !x.IsDeleted),

        TotalPatients = await _unitOfWork
            .Repository<Patient>()
            .CountAsync(x => !x.IsDeleted),

        TotalAppointments = await _unitOfWork
            .Repository<Appointment>()
            .CountAsync(x => !x.IsDeleted),

        TotalMedicalRecords = await _unitOfWork
            .Repository<MedicalRecord>()
            .CountAsync(x => !x.IsDeleted),

        TotalPrescriptions = await _unitOfWork
            .Repository<Prescription>()
            .CountAsync(x => !x.IsDeleted),

        TotalBills = await _unitOfWork
            .Repository<Billing>()
            .CountAsync(x => !x.IsDeleted),

        TotalPayments = await _unitOfWork
            .Repository<Payment>()
            .CountAsync(x => !x.IsDeleted),

        TotalRevenue = await _unitOfWork
            .Repository<Billing>()
            .SumAsync(
                x => !x.IsDeleted && x.PaymentStatus == "Paid",
                x => x.TotalAmount),

        PendingBills = await _unitOfWork
            .Repository<Billing>()
            .CountAsync(x => !x.IsDeleted && x.PaymentStatus == "Pending"),

        PaidBills = await _unitOfWork
            .Repository<Billing>()
            .CountAsync(x => !x.IsDeleted && x.PaymentStatus == "Paid"),

        ScheduledAppointments = await _unitOfWork
            .Repository<Appointment>()
            .CountAsync(x => !x.IsDeleted && x.Status == "Scheduled"),

        CompletedAppointments = await _unitOfWork
            .Repository<Appointment>()
            .CountAsync(x => !x.IsDeleted && x.Status == "Completed"),

        CancelledAppointments = await _unitOfWork
            .Repository<Appointment>()
            .CountAsync(x => !x.IsDeleted && x.Status == "Cancelled")
    };

    }
    // Real revenue-over-time data for the dashboard's Revenue Trend chart.
    // Billing has no dedicated "paid on" date, so CreatedOn (the closest
    // available field in the existing schema, no migration needed) is used
    // to bucket paid bills into their calendar month.
    private async Task<List<MonthlyRevenuePointDto>> GetMonthlyRevenueAsync()
    {
        var monthCount = 6;

        // First day of the month, (monthCount - 1) months back — e.g. with
        // monthCount = 6 and today in March, this is October 1st.
        var rangeStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
            .AddMonths(-(monthCount - 1));

        // Filtered at the query level so only the relevant recent rows are
        // fetched, not the whole Billing table.
        var paidBillsInRange = await _unitOfWork
            .Repository<Billing>()
            .GetAllAsync(x =>
                !x.IsDeleted &&
                x.PaymentStatus == "Paid" &&
                x.CreatedOn >= rangeStart);

        var revenueByMonth = paidBillsInRange
            .GroupBy(x => new DateTime(x.CreatedOn.Year, x.CreatedOn.Month, 1))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

        var points = new List<MonthlyRevenuePointDto>();

        for (var i = 0; i < monthCount; i++)
        {
            var monthStart = rangeStart.AddMonths(i);

            points.Add(new MonthlyRevenuePointDto
            {
                Month = monthStart.ToString("MMM yyyy"),
                Revenue = revenueByMonth.TryGetValue(monthStart, out var revenue) ? revenue : 0m
            });
        }

        return points;
    }
}

