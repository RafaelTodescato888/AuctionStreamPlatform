using Gateway.WebApi.Configurations;
using Gateway.WebApi.Extensions.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddReverseProxy()
    .LoadFromMemory(RouteExtensions.ConfigureRoutes(), ClusterExtensions.ConfigureClusters());

builder.Services.ConfigureRolesPolicyValidation(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();
