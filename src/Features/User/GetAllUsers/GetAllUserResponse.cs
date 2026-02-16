namespace Shipment.Features.User.GetAllUsers;

public record class GetAllUserResponse(string FirstName, string LastName, string Username, DateTime Birthday);