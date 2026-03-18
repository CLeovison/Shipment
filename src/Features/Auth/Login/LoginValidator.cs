using FluentValidation;

namespace Shipment.Features.Auth.Login;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
            RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required / missinng");

            RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Please provide your password");
    }
}