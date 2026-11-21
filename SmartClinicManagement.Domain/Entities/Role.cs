using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class Role : BaseEntity
{
    public int RoleID { get; set; }

    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RoleActivities> RoleActivities { get; set; }
       = new List<RoleActivities>();
}