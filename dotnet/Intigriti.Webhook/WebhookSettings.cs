using System.ComponentModel.DataAnnotations;

namespace Intigriti.Webhook;

public record WebhookSettings
{
    [Required]
    public required string Secret { get; init; }
}
