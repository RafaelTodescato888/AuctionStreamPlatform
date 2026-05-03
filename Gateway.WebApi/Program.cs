using Gateway.WebApi.Extensions.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromMemory(RouteExtensions.ConfigureRoutes(), ClusterExtensions.ConfigureClusters());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapReverseProxy();

app.Run();
