namespace SmartClinicManagement.Application.DTOs.Dashboard;

public class DashboardDto
{
    public int TotalUsers { get; set; }

    public int TotalDepartments { get; set; }

    public int TotalRoles { get; set; }

    public int TotalDoctors { get; set; }

    public int TotalPatients { get; set; }

    public int TotalAppointments { get; set; }

    public int TotalMedicalRecords { get; set; }

    public int TotalPrescriptions { get; set; }

    public int TotalBills { get; set; }

    public int TotalPayments { get; set; }

    public decimal TotalRevenue { get; set; }

    public int PendingBills { get; set; }

    public int PaidBills { get; set; }

    public int ScheduledAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public int CancelledAppointments { get; set; }

    // Real paid-billing revenue for the last 6 months (oldest first), used
    // by the dashboard's Revenue Trend chart. Replaces the old "Doctors vs
    // Patients" chart with something that actually shows a trend over time.
    public List<MonthlyRevenuePointDto> MonthlyRevenue { get; set; } = new();
}

public class MonthlyRevenuePointDto
{
    // Short display label, e.g. "Mar 2026".
    public string Month { get; set; } = string.Empty;

    public decimal Revenue { get; set; }
}
