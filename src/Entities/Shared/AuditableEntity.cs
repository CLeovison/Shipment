namespace Shipment.Entities.Shared;

public abstract class AuditableEntity
{
    public DateTime CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}