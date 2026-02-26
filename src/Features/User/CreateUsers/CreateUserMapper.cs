using Microsoft.AspNetCore.Identity;
using Shipment.Entities;

namespace Shipment.Features.User.CreateUsers;

public record class CreateUserRequest(string FirstName, string LastName, string Username, string Password, string ConfirmPassword, DateTime Birthday);
public record class CreateUserResponse(string FirstName, string LastName, string Username, DateTime Birthday);


public static class CreateUserMapper
{

    public static Users ToUserCreateRequest(this CreateUserRequest request)
    {
        return new Users
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Username = request.Username,

            Birthday = request.Birthday
        };
    }

    public static CreateUserResponse ToCreateUserResponse(this Users user)
    {
        return new CreateUserResponse(
            user.FirstName,
            user.LastName,
            user.Username,
            user.Birthday
        );
    }

}