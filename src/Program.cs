using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddSignalR();

builder.Services.AddUserHandler();
builder.Services.AddShipmentHandler();
builder.Services.Auth();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.Endpoint();

app.Run();
