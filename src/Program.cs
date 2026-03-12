using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Extensions;
using Shipment.Hubs;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddSignalR();

builder.Services.AddUserHandler();
builder.Services.AddShipmentHandler();
builder.Services.Auth(configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithOrigins("http://127.0.0.1:5500")); 
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseRouting();       
app.UseCors();           
app.UseAuthentication();
app.UseAuthorization();


app.Endpoint(); 
app.MapHub<ShipmentNotificationHub>("/hubs/shipments"); 

app.Run();