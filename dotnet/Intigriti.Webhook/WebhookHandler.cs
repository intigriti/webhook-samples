using System.Text.Json;
using Intigriti.Webhook.Events;

namespace Intigriti.Webhook;

public interface IWebhookHandler
{
    Task<bool> HandleWebhookAsync(string eventType, Stream body);
}

internal class WebhookHandler : IWebhookHandler
{
    private readonly ILogger<WebhookHandler> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public WebhookHandler(ILogger<WebhookHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleWebhookAsync(string eventType, Stream body)
    {
        try
        {
            var submissionEvent = await DeserializeEventAsync(eventType, body);
            if (submissionEvent == null) return false;

            var payload = JsonSerializer.Serialize(submissionEvent, JsonOptions);
            _logger.LogInformation("Received event {EventType}, Payload: {Payload}",
                submissionEvent.GetType().Name, payload);

            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError("Failed to deserialize {EventType}, Error: {Error}", 
                eventType, ex.Message);

            return false;
        }
    }

    private static async Task<SubmissionEvent?> DeserializeEventAsync(string eventType, Stream body)
    {
        return eventType switch
        {
            nameof(TestEvent) => await JsonSerializer.DeserializeAsync<TestEvent>(body, JsonOptions),
            nameof(SubmissionCreated) => await JsonSerializer.DeserializeAsync<SubmissionCreated>(body, JsonOptions),
            nameof(SubmissionSeverityChanged) => await JsonSerializer.DeserializeAsync<SubmissionSeverityChanged>(body, JsonOptions),
            nameof(SubmissionStatusChanged) => await JsonSerializer.DeserializeAsync<SubmissionStatusChanged>(body, JsonOptions),
            nameof(SubmissionMessagePlaced) => await JsonSerializer.DeserializeAsync<SubmissionMessagePlaced>(body, JsonOptions),
            _ => null
        };
    }
}
