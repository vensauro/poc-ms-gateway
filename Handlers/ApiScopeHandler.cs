using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

public class ApiScopeHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiKeySettings _apiKeySettings;
    private readonly IConfiguration _config;

    public ApiScopeHandler(
        IHttpContextAccessor httpContextAccessor,
        IOptions<ApiKeySettings> apiKeySettings,
        IConfiguration config)
    {
        _httpContextAccessor = httpContextAccessor;
        _apiKeySettings = apiKeySettings.Value;
        _config = config;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is null");

        // Identifica API baseada no path: /{api}/...
        var apiName = ExtractApiNameFromPath(context.Request.Path);

        if (string.IsNullOrEmpty(apiName))
            return Error(HttpStatusCode.InternalServerError, "Unable to determine API name from path");

        // API KEY já validada pelo Authentication Handler
        if (!(context.User?.Identity?.IsAuthenticated ?? false))
            return Error(HttpStatusCode.Unauthorized, "Missing or invalid X-Api-Key");

        // Identificar escopo automaticamente
        // Claims esperadas: "scope:Tasks" = Private | Public
        var scope = GetAutomaticScopeForApi(context.User.Claims, apiName);

        if (scope == null)
            return Error(HttpStatusCode.Forbidden, $"Client does not have scope for API '{apiName}'");

        if (scope.Equals("Private", StringComparison.OrdinalIgnoreCase))
        {
            var jwtResult = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

            if (!jwtResult.Succeeded)
                return Error(HttpStatusCode.Unauthorized, "Missing or invalid user JWT (Private scope)");

            // Injeta JWT de máquina real
            var machineJwt = GenerateMachineJwt();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", machineJwt);
        }
        else
        {
            var internalKey = GetInternalApiKeyFor(apiName);

            if (!request.Headers.Contains("X-Api-Key"))
                request.Headers.Add("X-Api-Key", internalKey);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string ExtractApiNameFromPath(PathString path)
    {
        var trimmed = path.HasValue ? path.Value!.Trim('/') : "";
        if (string.IsNullOrEmpty(trimmed)) return "";

        var root = trimmed.Split('/')[0];
        return Capitalize(root);
    }

    private string? GetAutomaticScopeForApi(IEnumerable<Claim> claims, string apiName)
    {
        var claim = claims.FirstOrDefault(c =>
            c.Type.StartsWith("scope:", StringComparison.OrdinalIgnoreCase)
            && c.Type.EndsWith(apiName, StringComparison.OrdinalIgnoreCase));

        return claim?.Value;
    }

    private string GetInternalApiKeyFor(string apiName)
    {
        return _apiKeySettings.Keys.First().Key;
    }

    private string GenerateMachineJwt()
    {
        var issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not set");
        var audience = _config["Jwt:MachineAudience"] ?? "internal-api";
        var secret = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not set");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: new[] { new Claim("sub", "gateway-machine") },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s[1..];
    }

    private HttpResponseMessage Error(HttpStatusCode status, string msg)
    {
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(
                $"{{\"message\":\"{msg}\"}}",
                Encoding.UTF8,
                "application/problem+json")
        };
    }
}
