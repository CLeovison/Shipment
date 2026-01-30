using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Extensions;
using Shipment.Features.User.CreateUsers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<CreateUserHandler>();
var app = builder.Build();

app.Endpoint();

app.Run();
