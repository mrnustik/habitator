using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var oltpEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"];

var openTelemetryBuilder = builder.Services.AddOpenTelemetry();

openTelemetryBuilder.ConfigureResource(r => r.AddService("habitator-api"));

openTelemetryBuilder.WithMetrics(m =>
                                     m.AddAspNetCoreInstrumentation()
                                      .AddRuntimeInstrumentation()
                                      .AddProcessInstrumentation()
                                      .AddMeter("*")
                                      .AddPrometheusExporter());

openTelemetryBuilder.WithTracing(t =>
{
    t.AddAspNetCoreInstrumentation();
    if (!string.IsNullOrEmpty(oltpEndpoint))
    {
        t.AddOtlpExporter(e =>
        {
            e.Protocol = OtlpExportProtocol.Grpc;
            e.Endpoint = new Uri(oltpEndpoint);
        });
    }
});

builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeScopes = true;

    if (!string.IsNullOrEmpty(oltpEndpoint))
    {
        o.AddOtlpExporter(e =>
        {
            e.Protocol = OtlpExportProtocol.Grpc;
            e.Endpoint = new Uri(oltpEndpoint);
        });
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => TypedResults.Ok());
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("alive"),
});

app.MapPrometheusScrapingEndpoint();

app.Run();
