using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Encodings.Web;
using Microsoft.IdentityModel.Tokens;

namespace ApiGateway.BuildingBlocks.AccessControl.ApiAuth
{
    public class ApiAuthAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Auth";
        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;

        public string Issuer { get; set; } = "";
        public string Audience { get; set; } = "";
        public string Secret { get; set; } = "";
    }

    public static partial class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiAuthSupport(
        this AuthenticationBuilder builder,
        IConfiguration configuration)
        {
            return builder.AddScheme<ApiAuthAuthenticationOptions, ApiAuthAuthenticationHandler>(
                ApiAuthAuthenticationOptions.DefaultScheme,
                options =>
                {
                    options.Issuer = configuration["Jwt:Issuer"] ?? "";
                    options.Audience = configuration["Jwt:Audience"] ?? "";
                    options.Secret = configuration["Jwt:Secret"] ?? "";
                });
        }
    }

    public class ApiAuthAuthenticationHandler : AuthenticationHandler<ApiAuthAuthenticationOptions>
    {
        public ApiAuthAuthenticationHandler(
            IOptionsMonitor<ApiAuthAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));
            }

            var header = authHeader.ToString();
            if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header"));
            }

            var token = header["Bearer ".Length..].Trim();

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Options.Secret)),

                ValidateIssuer = true,
                ValidIssuer = Options.Issuer,

                ValidateAudience = true,
                ValidAudience = Options.Audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParams, out _);

                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                return Task.FromResult(AuthenticateResult.Fail($"JWT validation failed: {ex.Message}"));
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 401;
            Response.ContentType = "application/problem+json";

            await Response.WriteAsync("""
            {"message":"Unauthorized - Invalid or missing Bearer token"}
            """);
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = 403;
            Response.ContentType = "application/problem+json";

            await Response.WriteAsync("""
            {"message":"Forbidden - Token does not have access"}
            """);
        }
    }
}
