using Microsoft.EntityFrameworkCore;

namespace Shipment.Database;


public sealed class AppDbContext(DbContextOptions options) : DbContext(options)
{
    
}