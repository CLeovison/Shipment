using System.Reflection;
using Shipment.Abstract;

namespace Shipment.Extensions;

public static class EndpointExtension
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.DefinedTypes
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        foreach (var type in types)
        {
            services.AddScoped(typeof(IEndpoint), type); 
        }

        return services;
    }

    public static WebApplication Endpoint(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        var routeBuilder = (IEndpointRouteBuilder)app;

        using var scope = app.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetServices<IEndpoint>();
        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.Endpoint(routeBuilder);
        }

        return app;
    }

    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder app, string permission)
    {
        return app.RequireAuthorization(permission);
    }
}