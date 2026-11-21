using SmartClinicManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SmartClinicManagement.Domain.Entities;

public class Activity:BaseEntity
{
    public int ActivityID { get; set; }

    public string ActivityName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? MenuURL { get; set; }

    public string Status { get; set; } = "Active";

    // Navigation Property
    public ICollection<RoleActivities> RoleActivities { get; set; } = new List<RoleActivities>();
}