using FluentValidation;

namespace Shipment.Extensions;

public class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Dynamically find the argument that matches the type TRequest
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
        {
            // If the expected request type is not found, you might want to log this or return a 400.
            // This can happen if the endpoint signature doesn't include TRequest.
            return TypedResults.BadRequest($"Expected request of type {typeof(TRequest).Name} was not found in the endpoint arguments.");
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        if (!result.IsValid)
        {
            return TypedResults.ValidationProblem(result.ToDictionary());
        }

        return await next(context);
    }
}

public static class ValidationExtension
{
    /// <summary>
    /// Adds FluentValidation-based endpoint filtering to the route handler.
    /// Applies the <see cref="ValidationFilter{TRequest}"/> to validate incoming requests of type <typeparamref name="TRequest"/>.
    /// Automatically documents the endpoint to produce a 400 validation problem response in OpenAPI metadata.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request model to be validated.</typeparam>
    /// <param name="builder">The route handler builder to extend.</param>
    /// <returns>The modified <see cref="RouteHandlerBuilder"/> with validation filtering applied.</returns>

    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<ValidationFilter<TRequest>>().ProducesValidationProblem();
    }
}