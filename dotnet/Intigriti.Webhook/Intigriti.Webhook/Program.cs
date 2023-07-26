using System.Text.Json;
using Intigriti.Webhook;
using Intigriti.Webhook.Events;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Logging
    .ClearProviders()
    .AddSerilog(logger);

builder.Services.Configure<Settings>(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<AuthenticationMiddleware>();

app.MapPost("/", async (HttpContext httpContext, ILogger<Program> logger) =>
{
    var eventType = httpContext.Request.Headers["x-intigriti-event"].ToString();
    
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    SubmissionEvent? submissionEvent =  eventType switch
    {
        nameof(TestEvent) => await JsonSerializer.DeserializeAsync<TestEvent>(httpContext.Request.Body, options),
        nameof(SubmissionCreated) => await JsonSerializer.DeserializeAsync<SubmissionCreated>(httpContext.Request.Body, options),
        nameof(SubmissionSeverityChanged) => await JsonSerializer.DeserializeAsync<SubmissionSeverityChanged>(httpContext.Request.Body, options),
        nameof(SubmissionStatusChanged) => await JsonSerializer.DeserializeAsync<SubmissionStatusChanged>(httpContext.Request.Body, options),
        nameof(SubmissionMessagePlaced) => await JsonSerializer.DeserializeAsync<SubmissionMessagePlaced>(httpContext.Request.Body, options),
        _ => null
    };

    if (submissionEvent == null) return Results.BadRequest();

    var payload = JsonSerializer.Serialize(submissionEvent, options);

    logger.LogInformation("Received event {eventType} \n {payload}", submissionEvent.GetType().Name, payload);

    return Results.NoContent();
});

app.Run();