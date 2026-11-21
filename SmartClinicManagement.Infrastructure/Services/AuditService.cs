using SmartClinicManagement.Application.Interfaces;
using SmartClinicManagement.Domain.Entities;

namespace SmartClinicManagement.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(
        int userId,
        string actionType,
        string tableName,
        int recordId,
        string? oldValue = null,
        string? newValue = null,
        string? ipAddress = null,
        string? remarks = null)
    {
        var log = new AuditLog
        {
            UserID = userId,
            ActionType = actionType,
            TableName = tableName,
            RecordID = recordId,
            OldValue = oldValue,
            NewValue = newValue,
            IPAddress = ipAddress,
            Remarks = remarks,
            ActionDate = DateTime.Now
        };

        await _unitOfWork
            .Repository<AuditLog>()
            .AddAsync(log);

        await _unitOfWork.SaveChangesAsync();
    }
}