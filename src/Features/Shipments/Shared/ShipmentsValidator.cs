using FluentValidation;
using Shipment.Features.Shipments.Shared;

namespace Shipment.Features.Shipments.CreateShipments;

public sealed class ShipmentValidation : AbstractValidator<ShipmentRequest>
{
    public ShipmentValidation()
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