using Microsoft.AspNetCore.Mvc;
using Intigriti.Webhook;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<WebhookSettings>()
    .BindConfiguration("WebhookSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddScoped<IWebhookHandler, WebhookHandler>();

var app = builder.Build();

app.UseMiddleware<AuthenticationMiddleware>();

app.MapPost("/", async (
    [FromHeader(Name = "x-intigriti-event")] string eventType,
    HttpRequest request,
    IWebhookHandler handler,
    ILogger<Program> logger) =>
{
    try
    {
        var result = await handler.HandleWebhookAsync(eventType, request.Body);
        return result ? Results.NoContent() : Results.BadRequest();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing webhook");
        return Results.StatusCode(500);
    }
});

app.Run();
