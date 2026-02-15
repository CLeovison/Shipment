namespace Shipment.Features.User.UpdateUsers;


public record class UpdateUserRequest(int UserId, string FirstName, string LastName, string Username, string Password, DateTime Birthday, DateTime UpdateAt);