using System.ComponentModel.DataAnnotations;

namespace SmartClinicManagement.Application.DTOs.Permissions;

public class UpdatePermissionDto
{
    [Required]
    public int PermissionID { get; set; }

    public bool CanCreate { get; set; }

    public bool CanRead { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }
}