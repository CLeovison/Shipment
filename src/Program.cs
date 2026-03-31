using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSingleton<UploadProgressStore>();

builder.Services.AddSignalR();
builder.Services.AddHostedServices();
builder.Services.AddUserHandler();
builder.Services.AddShipmentHandler();
builder.Services.AddCorsPolicy();

builder.Services.Auth(configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();


var app = builder.Build();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.Endpoint();
app.MapHub<ShipmentNotificationHub>("/hubs/shipments");

app.Run();
