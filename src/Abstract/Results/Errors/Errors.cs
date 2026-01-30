namespace Shipment.Abstract.Results.Errors;

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");
    public static readonly Error NotFound = new("Error.NotFound", "The requested resource was not found.");
    public static readonly Error Unauthorized = new("Error.Unauthorized", "You are not authorized to perform this action.");
    public static readonly Error Cancelled = new("Error.Cancelled","Operation was cancelled");

    public static Error AlreadyExists(string entity) => new("Error.AlreadyExist", $"The {entity} already exists");
    public static Error DidntExists(string entity) => new("Error.Didn't Exists", $"The {entity} didn't exists");
}