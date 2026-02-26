using Shipment.Entities;

namespace Shipment.Features.User.UpdateUsers;

public static class UpdateUserMapper
{
    public static void ToEntity(this UpdateUserRequest request, Users existing, int id)
    {
        existing.UserId = id;
        existing.FirstName = request.FirstName;
        existing.LastName = request.LastName;
        existing.Username = request.Username;
        existing.Password = request.Password;
        existing.Birthday = request.Birthday;
        existing.ModifiedAt = request.ModifiedAt;
    }

}