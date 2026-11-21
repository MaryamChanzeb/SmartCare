using SmartClinicManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartClinicManagement.Domain.Entities
{
    public class Permission: BaseEntity
    {
        [Key]
        public int PermissionID { get; set; }


        public int RoleActivityID { get; set; }

        public bool CanCreate { get; set; }

        public bool CanRead { get; set; }

        public bool CanUpdate { get; set; }

        public bool CanDelete { get; set; }


        public RoleActivities? RoleActivity { get; set; }


    }
}