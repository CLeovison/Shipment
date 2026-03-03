using FluentValidation;

namespace Shipment.Features.Shipments.CreateShipments;

public class CreateShipmentValidator : AbstractValidator<CreateShipmentRequest>
{
    public CreateShipmentValidator()
    {
        RuleFor(x => x.PurchaseOrderNumber)
           .NotEmpty()
           .WithMessage("Purchase Order Number is required.");

        RuleFor(x => x.Vendor)
            .NotEmpty()
            .WithMessage("Vendor name must be provided.");

        RuleFor(x => x.TimeOfArrival)
            .NotEmpty()
            .WithMessage("Time of Arrival cannot be empty.");
    }
}