using SmartClinicManagement.Domain.Common;

namespace SmartClinicManagement.Domain.Entities;

public class Department : BaseEntity
{
    public int DepartmentID { get; set; }

    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}