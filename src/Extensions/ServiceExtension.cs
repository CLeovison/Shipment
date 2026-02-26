using Shipment.Abstract.Messaging;

using Shipment.Features.Shipments.CreateShipments;
using Shipment.Features.Shipments.DeleteShipments;
using Shipment.Features.Shipments.GetAllShipments;
using Shipment.Features.Shipments.GetShipmentsById;
using Shipment.Features.Shipments.UpdateShipments;
using Shipment.Features.User.CreateUsers;
using Shipment.Features.User.DeleteUsers;
using Shipment.Features.User.GetAllUsers;
using Shipment.Features.User.GetUserById;
using Shipment.Features.User.UpdateUsers;
namespace Shipment.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssembliesOf(typeof(ServiceExtensions))
        .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()

        .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()

        .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

        return services;
    }

    public static IServiceCollection AddUserHandler(this IServiceCollection services)
    {
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<GetAllUserHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<UpdateUserHandler>();

        return services;
    }

    public static IServiceCollection AddShipmentHandler(this IServiceCollection services)
    {
        services.AddScoped<CreateShipmentHandler>();
        services.AddScoped<GetAllShipmentHandler>();
        services.AddScoped<GetShipmentByIdHandler>();
        services.AddScoped<UpdateShipmentHandler>();
        services.AddScoped<DeleteShipmentHandler>();
        return services;
    }
}