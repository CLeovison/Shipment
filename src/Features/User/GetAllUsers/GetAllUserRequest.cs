namespace Shipment.Features.User.GetAllUsers;

public record class GetAllUserRequest(string FirstName, string LastName, string Username, DateTime Birthday);