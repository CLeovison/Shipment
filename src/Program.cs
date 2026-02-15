using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shipment.Database;
using Shipment.Extensions;
using Shipment.Features.User.CreateUsers;
using Shipment.Features.User.DeleteUsers;
using Shipment.Features.User.GetUserById;
using Shipment.Features.User.UpdateUsers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());


builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<UpdateUserHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();


var app = builder.Build();

app.Endpoint();

app.Run();
