using Shipment.Abstract.Results;
using Shipment.Abstract.Results.Errors;
using Shipment.Database;
using Shipment.Entities;

namespace Shipment.Features.User.DeleteUsers;

internal sealed class DeleteUserHandler(AppDbContext dbContext)
{
    public async Task<Result<Users>> DeleteUserAsync(int id, CancellationToken ct)
    {
        try
        {
            var existing = await dbContext.Users.FindAsync([id], ct); // [id] for params overload

            if (existing is null)
            {
                return Result.Failure<Users>(Error.DidntExists("user"));
            }

            dbContext.Users.Remove(existing);
            await dbContext.SaveChangesAsync(ct);

            return Result.Success(existing);
        }
        catch (Exception)
        {
            return Result.Failure<Users>(
                new Error("Error.Unexpected", "An unexpected error occurred while deleting the user.")
            );
        }
    }
}