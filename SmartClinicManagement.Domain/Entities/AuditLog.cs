using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int AuditID { get; set; }

    public int UserID { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string TableName { get; set; } = string.Empty;

    public int RecordID { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.Now;

    public string? IPAddress { get; set; }

    public string? Remarks { get; set; }
}