using FluentValidation;
using Shipment.Features.User.CreateUsers;

namespace Shipment.Features.User.CreateUsers;


public sealed class CreateUserValidation : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidation()
    {
        RuleFor(user => user.FirstName).NotEmpty().WithMessage("Please Fill the First Name");

        RuleFor(user => user.LastName).NotEmpty().WithMessage("Please Fill the Last Name");

        RuleFor(user => user.Username).MinimumLength(5).NotEmpty().WithMessage("The Minimum Length for Username is 5 Characters");

        RuleFor(user => user.Password)
        .NotEmpty().Must(p => !string.IsNullOrWhiteSpace(p)).WithMessage("Please Fill the Password")
        .MinimumLength(8).WithMessage("Minimum length is 8 characters")
        .Matches(@"[A-Z]+").WithMessage("Password must contain uppercase")
        .Matches(@"[a-z]+").WithMessage("Password must contain lowercase")
        .Matches(@"[0-9]+").WithMessage("Password must contain number");

        RuleFor(user => user.ConfirmPassword).Equal(user => user.Password).WithMessage("Your confirmation password must match the password you entered.");


    }
}