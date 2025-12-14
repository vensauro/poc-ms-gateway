using System.Net;
using Microsoft.Extensions.Options;

public class ApiScopeHandler : DelegatingHandler
{
    private readonly ApiKeySettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiScopeHandler(
        IOptions<ApiKeySettings> settings,
        IHttpContextAccessor httpContextAccessor)
    {
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var routeScope = GetRouteScope(request);

        if (string.IsNullOrWhiteSpace(routeScope))
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Route scope not configured."
            };

        if (!request.Headers.TryGetValues("X-Api-Key", out var keys))
            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Missing X-Api-Key"
            };

        var providedKey = keys.First();
        var client = _settings.Keys.FirstOrDefault(k => k.Key == providedKey);

        if (client == null)
            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Invalid X-Api-Key"
            };

        if (!client.Scopes.Contains(routeScope))
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                ReasonPhrase = "Scope not allowed for this client"
            };
        }

        bool isPrivate = routeScope.EndsWith("[Private]");

        if (isPrivate)
        {
            if (!request.Headers.Authorization?.Scheme.Equals("Bearer") ?? true)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    ReasonPhrase = "Missing Bearer token"
                };
            }

            var accessToken = request.Headers.Authorization.Parameter;

            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string GetRouteScope(HttpRequestMessage request)
    {
        if (request.Properties.TryGetValue("DownstreamRoute", out var routeObj))
        {
            dynamic route = routeObj;
            return route?.Metadata?.Scope;
        }

        return null;
    }
}
