using SmartClinicManagement.Application.DTOs.Dashboard;

namespace SmartClinicManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}