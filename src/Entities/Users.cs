using Shipment.Entities.Shared;

namespace Shipment.Entities;

public class Users : AuditableEntity
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }

    public ICollection<ShipmentDetails> Shipments { get; set; } = new List<ShipmentDetails>();
}