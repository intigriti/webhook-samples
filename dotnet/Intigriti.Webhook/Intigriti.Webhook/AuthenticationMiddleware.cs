using Microsoft.Extensions.Options;

namespace Intigriti.Webhook;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Settings _settings;

    public AuthenticationMiddleware(RequestDelegate next, IOptions<Settings> settings)
    {
        _next = next;
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public Task Invoke(HttpContext ctx)
    {
        // TODO check signature
        return _next.Invoke(ctx);
    }
}