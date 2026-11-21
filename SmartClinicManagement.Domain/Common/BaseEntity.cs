namespace SmartClinicManagement.Domain.Common;

public abstract class BaseEntity
{
    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool IsDeleted { get; set; }

    public int? CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }
}