using User.Infrastructure.Extensions;
using User.WebApi.Extensions.Endpoints;
using User.Application.Extensions.UseCases;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .ConfigureInfrastructure(builder.Configuration)
    .AddUseCases();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Services.ConfigureMigrations();

app.MapEndpoints();

app.Run();
