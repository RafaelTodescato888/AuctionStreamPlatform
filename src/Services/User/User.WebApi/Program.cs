using CrossCutting.Extensions;
using CrossCutting.Log.Common.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using User.Application.Extensions.UseCases;
using User.Domain.Constants.Configuration;
using User.Domain.Constants.RateLimitStrategy;
using User.Domain.Constants.Service;
using User.Infrastructure.Extensions;
using User.WebApi.Extensions.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<TokenGenerationConfig>(builder.Configuration.GetSection(nameof(TokenGenerationConfig)));
builder.Services.Configure<AdminCredentialsConfig>(builder.Configuration.GetSection(nameof(AdminCredentialsConfig)));
builder.Services.Configure<LoginAttemptsConfig>(builder.Configuration.GetSection(nameof(LoginAttemptsConfig)));

builder.Services.AddOpenApi();

builder.Services.AddJwtCustomConfiguration(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services
    .ConfigureInfrastructure(builder.Configuration)
    .ConfigureRateLimiters()
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

app.UseRateLimiter();

app.MapEndpoints();

app.Run();
