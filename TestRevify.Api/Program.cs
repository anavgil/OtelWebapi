using Scalar.AspNetCore;
using Serilog;
using Serilog.Enrichers.Span;
using System.Text.Json.Serialization;
using TestRevify.Api.Endpoints;
using TestRevify.Api.Middlewares;
using TestRevify.Api.OTL;
using TestRevify.Api.Workers;

var serviceName = "Revify-WebApi";
var builder = WebApplication.CreateSlimBuilder(args);
//const string outputTemplate =
//    "[{Level:w}]: {Timestamp:dd-MM-yyyy:HH:mm:ss} {MachineName} {EnvironmentName} {SourceContext} {Message}{NewLine}{Exception}";

builder.Logging.ClearProviders();

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadName()
    .ReadFrom.Services(services)
    .WriteTo.Console()
    .WriteTo.OpenTelemetry(options =>
    {
        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
#if DEBUG
        options.Endpoint = "http://localhost:4317";
        options.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = serviceName,
            ["deployment.environment"] = "Develop"
        };
#else
        options.Endpoint = "http://otel-collector:4317";
        options.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = serviceName,
            ["deployment.environment"] = "Production"
        };
#endif
    })
    );

builder.Services.RegisterOtl(serviceName);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

//builder.Services.AddHostedService<QueuedHostedService>();
//builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
//{
//    if (!int.TryParse(builder.Configuration["QueueCapacity"], out var queueCapacity))
//        queueCapacity = 100;
//    return new BackgroundTaskQueue(queueCapacity);
//});

var app = builder.Build();

// Habilitar WebSockets
var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromMinutes(2), // Mantiene viva la conexión
};
app.UseWebSockets(webSocketOptions);
// Middleware personalizado para manejar las conexiones WebSocket
app.UseMiddleware<WebSocketMiddleware>();


app.UseSerilogRequestLogging();

app.RegisterTodoItemsEndpoints();
app.RegisterNativeEndpoints();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
    .WithTheme(ScalarTheme.Kepler)
    .WithLayout(ScalarLayout.Modern);
});

app.UseHttpsRedirection();

app.Run();

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
