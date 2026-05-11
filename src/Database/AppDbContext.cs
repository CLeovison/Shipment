using Microsoft.EntityFrameworkCore;
using Shipmennt.Entities;
using Shipment.Entities;

namespace Shipment.Database;


public sealed class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public required DbSet<Users> Users { get; set; }
    public required DbSet<ShipmentDetails> Shipments { get; set; }
    public required DbSet<RefreshTokens> RefreshToken { get; set; }
    public required DbSet<Role> Roles { get; set; }
    public required DbSet<Permission> Permissions { get; set; }
    public required DbSet<UploadLog> UploadLogs { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}