using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Intigriti.Webhook;

public class AuthenticationMiddleware
{
    private const string SignatureHeaderKey = "x-intigriti-digest";

    private readonly string _secret;
    private readonly ILogger<AuthenticationMiddleware> _logger;    
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(IOptions<WebhookSettings> settings, ILogger<AuthenticationMiddleware> logger, RequestDelegate next)
    {
        _secret = settings.Value.Secret;
        _logger = logger;
        _next = next;
    }

    public async Task Invoke(HttpContext ctx)
    {
        var body = await GetRequestBodyAsync(ctx.Request);

        var actualSignature = ctx.Request.Headers[SignatureHeaderKey].ToString();
        var expectedSignature = ComputeSignature(body, _secret);

        if (!actualSignature.Equals(expectedSignature))
        {
            _logger.LogInformation("Invalid signature");
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next.Invoke(ctx);
    }

    private static string ComputeSignature(string content, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashedBytes = hmac.ComputeHash(bytes);
        return Convert.ToBase64String(hashedBytes);
    }

    private static async Task<string> GetRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanSeek)
            request.EnableBuffering();

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync().ConfigureAwait(false);
        request.Body.Position = 0;

        return body;
    }
}
