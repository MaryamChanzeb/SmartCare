using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class PasswordDetails : BaseEntity
{
    public int PasswordID { get; set; }

    public int UserID { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsCurrent { get; set; }

    public string? SessionID { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public DateTime? LastLogoutTime { get; set; }
    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public User User { get; set; } = null!;
}