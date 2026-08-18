using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Gta.Application.Api.Authentication;

public sealed record DemoAccessGateOptions(bool Enabled, string Username, string Password);

public static class DemoAccessGate
{
    public static IServiceCollection AddDemoAccessGate(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var enabled = configuration.GetValue<bool>("DemoAccess:Enabled");
        var username = configuration["DemoAccess:Username"] ?? string.Empty;
        var password = configuration["DemoAccess:Password"] ?? string.Empty;

        if (enabled && (username.Length < 3 || password.Length < 16))
        {
            throw new InvalidOperationException(
                "Demo access requires a username of at least 3 characters and a password of at least 16 characters.");
        }

        services.AddSingleton(new DemoAccessGateOptions(enabled, username, password));
        return services;
    }
}

public sealed class DemoAccessGateMiddleware(
    RequestDelegate next,
    DemoAccessGateOptions options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!options.Enabled || context.Request.Path.StartsWithSegments("/health"))
        {
            await next(context);
            return;
        }

        if (TryReadCredentials(context.Request.Headers.Authorization.ToString(), out var username, out var password) &&
            FixedTimeEquals(username, options.Username) &&
            FixedTimeEquals(password, options.Password))
        {
            await next(context);
            return;
        }

        context.Response.Headers.WWWAuthenticate = "Basic realm=\"GTA Demo\", charset=\"UTF-8\"";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private static bool TryReadCredentials(
        string authorization,
        out string username,
        out string password)
    {
        username = string.Empty;
        password = string.Empty;

        if (!AuthenticationHeaderValue.TryParse(authorization, out var header) ||
            !header.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(header.Parameter))
        {
            return false;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter));
            var separator = decoded.IndexOf(':');
            if (separator < 1)
            {
                return false;
            }

            username = decoded[..separator];
            password = decoded[(separator + 1)..];
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool FixedTimeEquals(string supplied, string expected)
    {
        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(suppliedHash, expectedHash);
    }
}
