using Microsoft.EntityFrameworkCore;
using Shipment.Abstract;
using Shipment.Database;
using Shipment.Entities.Shared;

namespace Shipment.Features.User.GetAllUsers;

internal sealed class GetAllUserHandler(AppDbContext dbContext)
{
    public async Task<PaginationResponse<GetAllUserResponse>> GetAllUserAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        UserFilter filter,
        CancellationToken ct)
    {
        var query = dbContext.Users.AsQueryable();

        var lowerCase = searchTerm?.Trim().ToLower();

        if (!string.IsNullOrWhiteSpace(lowerCase))
        {
            query = query.Where(x =>
                x.Username.Contains(lowerCase) ||
                x.FirstName.Contains(lowerCase) ||
                x.LastName.Contains(lowerCase));
        }
        if (!string.IsNullOrWhiteSpace(filter.FirstName))
        {
            query = query.Where(x => x.FirstName.Contains(filter.FirstName));
        }

        if (!string.IsNullOrWhiteSpace(filter.Username))
        {
            query = query.Where(x => x.Username.Contains(filter.Username));
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
        .OrderBy(x => x.Username)
        .ThenBy(x => x.FirstName)
        .Skip((pageSize - 1) * pageNumber)
        .Take(pageNumber)
        .Select(u => new GetAllUserResponse(
                u.FirstName,
                u.LastName,
                u.Username,
                u.Birthday
        ))
        .ToListAsync(ct); ;


        return new PaginationResponse<GetAllUserResponse>(users, pageSize, pageNumber, totalCount);
    }
}


public sealed class GetAllUserEndpoint : IEndpoint
{
    public void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/users", async (GetAllUserHandler handler, [AsParameters] UserFilter filter, CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null) =>
        {
            var users = await handler.GetAllUserAsync(
                            pageSize,
                            pageNumber,
                            searchTerm,
                            filter,
                            ct);

            return Results.Ok(users);
        });
    }
}