
using Microsoft.AspNetCore.Identity;
using Shipment.Entities;

namespace Shipment.Features.Auth.Register;

public record class RegisterRequest(string FirstName,
string LastName,
string Username,
string Password,
string ConfirmPassword,
DateTime Birthday);

public static class RegisterMapper
{
    public static Users ToReqisterRequest(this RegisterRequest request)
    {
        return new Users
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Username = request.Username,
            Birthday = request.Birthday
        };
      
    }
}