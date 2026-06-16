using User.Application.Extensions.UseCases;
using User.Domain.Constants.Configuration;
using User.Infrastructure.Extensions;
using User.WebApi.Extensions.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthorization();

builder.Services
    .ConfigureAuthentication(builder.Configuration)
    .ConfigureInfrastructure(builder.Configuration)
    .AddHttpContextAccessor()
    .AddJsonStringEnumConverter()
    .AddServices()
    .AddUseCases();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Services.ConfigureMigrations();

app.MapEndpoints();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
