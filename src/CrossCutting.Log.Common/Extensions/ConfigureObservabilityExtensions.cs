using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using SerilogLog = Serilog.Log;

namespace CrossCutting.Log.Common.Extensions;

public static class ConfigureObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostBuilder host,
        string serviceName)
    {
        var lokiServiceUrl = Environment.GetEnvironmentVariable("LOKI_SERVICE_URL") ??
                configuration["LOKI_SERVICE_URL"] ??
                throw new ArgumentNullException("Could not find the LOKI_SERVICE_URL environment variable");

        SerilogLog.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.GrafanaLoki(lokiServiceUrl)
            .Enrich.FromLogContext()
            .CreateLogger();

        host.UseSerilog();

        var openTelemetryUrl = Environment.GetEnvironmentVariable("OPEN_TELEMETRY_URL") ??
            configuration["OPEN_TELEMETRY_URL"] ??
            throw new ArgumentNullException("Could not find the OPEN_TELEMETRY_URL environment variable");

        services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("AuctionStreamPlatform")
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(openTelemetryUrl);
                        o.Protocol = OtlpExportProtocol.Grpc;
                    })
                    .AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            });
        return services;
    }
}
