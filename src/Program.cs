using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shipment.Background;
using Shipment.Database;
using Shipment.Extensions;
using Shipment.Features.Shipments.UploadShipments;
using Shipment.Hubs;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddSingleton<UploadShipmentQueue>();

builder.Services.AddHostedService<ShipmentNoticeWorker>();
builder.Services.AddHostedService<RefreshTokenWorker>();
builder.Services.AddHostedService<ShipmentsUploadWorker>();

builder.Services.AddSignalR();

builder.Services.AddUserHandler();
builder.Services.AddShipmentHandler();
builder.Services.Auth(configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddCorsPolicy();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseRouting();
app.UseCors();

app.UseAuthentication(); 
app.UseAuthorization();

app.Endpoint();
app.MapHub<ShipmentNotificationHub>("/hubs/shipments");

app.Run();
