using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        int userId,
        string actionType,
        string tableName,
        int recordId,
        string? oldValue = null,
        string? newValue = null,
        string? ipAddress = null,
        string? remarks = null);
}