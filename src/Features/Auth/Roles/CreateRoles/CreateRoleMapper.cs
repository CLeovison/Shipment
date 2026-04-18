using Shipmennt.Entities;

namespace Shipment.Features.Auth.Roles;

public record class RoleRequest(string RoleName, string Description);
public record class RoleResponse(int RoleId, string RoleName, string Description);


public static class CreateRoleMapper
{
    public static Role ToRoleRequest(this RoleRequest request)
    {
        return new Role
        {
            RoleName = request.RoleName,
            Description = request.Description
        };
    }

    public static RoleResponse ToRoleResponse(this Role role)
    {
        return new RoleResponse(
            role.RoleId,
            role.RoleName,
            role.Description
        );
    }
}