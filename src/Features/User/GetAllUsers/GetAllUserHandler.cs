using Microsoft.EntityFrameworkCore;
using Shipment.Abstract.Results;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.User.GetAllUsers;

internal sealed class GetAllUserHandler(AppDbContext dbContext)
{

    public async Task<Result<List<Users>>> Handler(int pageNumber, int pageSize, string? searchTerm, CancellationToken ct)
    {
        var query = dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x =>
                x.Username.Contains(searchTerm) ||
                x.FirstName.Contains(searchTerm) ||
                x.LastName.Contains(searchTerm));
        }

        var users = await query
        .OrderBy(x => x.Username)
        .ThenBy(x => x.FirstName)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct); ;

        return Result.Success(users);
    }
}