using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shipment.Abstract;
using Shipment.Abstract.Messaging;
using Shipment.Auth;
using Shipment.Entities;

using Shipment.Features.Auth.Login;
using Shipment.Features.Auth.Logout;
using Shipment.Features.Auth.RefreshToken;
using Shipment.Features.Auth.RevokeRefreshToken;
using Shipment.Features.Shipments.CreateShipments;
using Shipment.Features.Shipments.DeleteShipments;
using Shipment.Features.Shipments.GetAllShipments;
using Shipment.Features.Shipments.GetShipmentNotice;
using Shipment.Features.Shipments.GetShipmentsById;
using Shipment.Features.Shipments.UpdateShipments;
using Shipment.Features.Shipments.UploadShipments;
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
        services.AddScoped<GetShipmentNoticeHandler>();
        services.AddScoped<UpdateShipmentHandler>();
        services.AddScoped<DeleteShipmentHandler>();
        services.AddScoped<UploadShipmentsHandler>();
        return services;
    }

    public static IServiceCollection Auth(this IServiceCollection services, IConfiguration configuration)
    {
        services
        .AddAuthorization()
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {

            opt.RequireHttpsMetadata = false;
            opt.SaveToken = true;

            // Allow JWT in cookie for browser clients
            opt.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Check Authorization header first
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        // If missing, fallback to cookie
                        var tokenFromCookie = context.Request.Cookies["accessToken"];
                        if (!string.IsNullOrEmpty(tokenFromCookie))
                            context.Token = tokenFromCookie;
                    }
                    return Task.CompletedTask;
                }
            };

            opt.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!)),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddScoped<PasswordHasher<Users>>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<RevokeRefreshToken>();
        services.AddHttpContextAccessor();
        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithOrigins("http://127.0.0.1:5500"));
        });

        return services;
    }
}