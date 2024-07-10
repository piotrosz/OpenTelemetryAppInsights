using System.Diagnostics;
using System.Diagnostics.Metrics;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Custom metrics for the application
var greeterMeter = new Meter("OtPrGrYa.Example", "1.0.0");
var countGreetings = greeterMeter.CreateCounter<int>("greetings.count", description: "Counts the number of greetings");

// Custom ActivitySource for the application
var greeterActivitySource = new ActivitySource("OtPrGrJa.Example");

var openTelemetryBuilder = builder.Services.AddOpenTelemetry();

// Configure OpenTelemetry Resources with the application name
openTelemetryBuilder.ConfigureResource(resource => resource
    .AddService(serviceName: builder.Environment.ApplicationName));

openTelemetryBuilder.UseAzureMonitor();
openTelemetryBuilder.WithMetrics(metrics => metrics
    .AddMeter(greeterMeter.Name)
    .AddMeter("Microsoft.AspNetCore.Hosting")
    .AddMeter("Microsoft.AspNetCore.Server.Kestrel"));
openTelemetryBuilder.WithTracing(tracing =>
{
    tracing.AddSource(greeterActivitySource.Name);
});

var app = builder.Build();


app.MapGet("/", SendGreeting);

app.Run();

async Task<string> SendGreeting(ILogger<Program> logger)
{
    // Create a new Activity scoped to the method
    using var activity = greeterActivitySource.StartActivity("GreeterActivity");

    // Log a message
    logger.LogInformation("Sending greeting");

    // Increment the custom counter
    countGreetings.Add(1);

    // Add a tag to the Activity
    activity?.SetTag("greeting", "Hello World!");

    return "Hello World!";
}