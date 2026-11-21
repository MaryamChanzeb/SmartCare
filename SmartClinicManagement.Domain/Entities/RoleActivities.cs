using SmartClinicManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace SmartClinicManagement.Domain.Entities;

public class RoleActivities:BaseEntity
{
    [Key]
    public int RoleActivityID { get; set; }

    public int RoleID { get; set; }

    public int ActivityID { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? AssignedDate { get; set; }

    public string Status { get; set; } = "Active";

    // Navigation Properties
    public Role? Role { get; set; }

    public Activity? Activity { get; set; }

    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}