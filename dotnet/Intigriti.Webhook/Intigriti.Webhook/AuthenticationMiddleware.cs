using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Intigriti.Webhook;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _secret;

    public AuthenticationMiddleware(RequestDelegate next, IOptions<Settings> settings)
    {
        _next = next;
        _secret = settings.Value.Secret ?? throw new ArgumentNullException(nameof(_secret));
    }

    public async Task Invoke(HttpContext ctx)
    {
        var body = await GetRequestBodyAsync(ctx.Request);

        var actualSignature = ctx.Request.Headers["x-intigriti-digest"].ToString();
        var expectedSignature = ComputeSignature(body, _secret);

        if (!actualSignature.Equals(expectedSignature))
        {
            ctx.Response.StatusCode = 401;
            return;
        }

        await _next.Invoke(ctx);
    }

    private static string ComputeSignature(string content, string secret)
    {
        using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashedBytes = hmacsha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashedBytes);
    }

    private static async Task<string> GetRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanSeek)
            request.EnableBuffering();

        request.Body.Position = 0;
        var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync().ConfigureAwait(false);
        request.Body.Position = 0;

        return body;
    }
}