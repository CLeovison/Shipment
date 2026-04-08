namespace Shipmennt.Entities;


public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    // The purpose of a role description in a roles table (typically part of a Role-Based Access Control - RBAC - system) is to 
    // provide a human-readable explanation of what a particular role represents, what its purpose is, 
    // and which permissions it is intended to grant
    public string Description { get; set; } = string.Empty;
}