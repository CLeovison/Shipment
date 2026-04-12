using Shipmennt.Entities;

namespace Shipment.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public required Users User { get; set; }
    public required Role Roles { get; set; }
}