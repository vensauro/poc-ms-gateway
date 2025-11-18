using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ApiGateway.BuildingBlocks.AccessControl.ApiAuth
{
    public class ApiAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Auth";
        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;
    }

    public static partial class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiAuthSupport(this AuthenticationBuilder builder)
        {
            return builder.AddScheme<ApiAuthAuthenticationOptions, ApiAuthAuthenticationHandler>(
                ApiAuthAuthenticationOptions.DefaultScheme, _ => { });
        }
    }

    public class ApiAuthAuthenticationHandler : AuthenticationHandler<ApiAuthAuthenticationOptions>
    {
        private const string ProblemDetailsContentType = "application/problem+json";
        private readonly ApiKeySettings _settings;

        public ApiAuthAuthenticationHandler(
            IOptionsMonitor<ApiAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IOptions<ApiKeySettings> apiKeyOptions)
            : base(options, logger, encoder, clock)
        {
            _settings = apiKeyOptions.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_settings.Enable)
            {
                return Task.FromResult(AuthenticateResult.Fail("API Key authentication disabled"));
            }

            if (!Request.Headers.TryGetValue("X-Api-Key", out var providedKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));
            }

            var match = _settings.Keys.FirstOrDefault(k => k.Key == providedKey);
            if (match == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationMethod, $"{ApiAuthAuthenticationOptions.DefaultScheme}.Auth.Type"),
                new Claim(ClaimTypes.Name, match.Client),
                new Claim("Id", match.Id.ToString()),
                new Claim("AuthType", "api_key"),
                new Claim("Client", $"{match.Client}.Auth.Client")
            };

            foreach (var scope in match.Scopes)
            {
                try
                {
                    var parts = scope.Split('[', ']');
                    if (parts.Length >= 2)
                    {
                        var api = parts[0].Trim();
                        var access = parts[1].Trim();
                        claims.Add(new Claim($"scope:{api}", access));
                        claims.Add(new Claim("scope", $"{api}:{access}"));
                    }
                    else
                    {
                        claims.Add(new Claim("scope", scope));
                    }
                }
                catch
                {
                    claims.Add(new Claim("scope", scope));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = ProblemDetailsContentType;
            var problemDetails = new { message = "Unauthorized Key" };

            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            Response.ContentType = ProblemDetailsContentType;
            var problemDetails = new { message = "Access Denied" };

            await Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
