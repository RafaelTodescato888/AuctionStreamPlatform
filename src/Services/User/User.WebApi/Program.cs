using CrossCutting.Extensions;
using CrossCutting.Log.Common.Extensions;
using User.Application.Extensions.UseCases;
using User.Domain.Constants.Configuration;
using User.Domain.Constants.Service;
using User.Infrastructure.Extensions;
using User.WebApi.Extensions.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));
builder.Services.Configure<AdminConfig>(builder.Configuration.GetSection("AdminConfig"));

builder.Services.AddOpenApi();

builder.Services.AddJwtCustomConfiguration(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services
    .ConfigureInfrastructure(builder.Configuration)
    .AddObservability(builder.Configuration, builder.Host, ServiceNameConstants.Name)
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

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
