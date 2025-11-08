using System.IdentityModel.Tokens.Jwt;

public interface IJwtContext
{
    string? UserId { get; }
    string? DeviceToken { get; }
}

public class JwtContext(IHttpContextAccessor accessor) : IJwtContext
{
    private readonly IHttpContextAccessor _accessor = accessor;

    public string? UserId => ReadClaim("sub");
    public string? DeviceToken => ReadClaim("device_token");

    private string? ReadClaim(string claimType)
    {
        var header = _accessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        if (header is null) return null;

        var token = new JwtSecurityTokenHandler().ReadJwtToken(header.Replace("Bearer ", ""));
        return token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
