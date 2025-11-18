using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

public interface IJwtContext
{
    string? UserId { get; }
    string? DeviceToken { get; }
}


public class JwtContext : IJwtContext
{
    private readonly IHttpContextAccessor _accessor;

    public JwtContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? UserId => ReadClaim("sub");
    public string? DeviceToken => ReadClaim("device_token");

    private string? ReadClaim(string claimType)
    {
        var context = _accessor.HttpContext;
        if (context == null) return null;

        var jwtAuth = context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme).Result;
        if (jwtAuth?.Succeeded == true && jwtAuth.Principal != null)
        {
            var claim = jwtAuth.Principal.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
            if (claim != null) return claim;
        }

        var header = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(header)) return null;
        var raw = header.Replace("Bearer ", "");
        try
        {
            var token = new JwtSecurityTokenHandler().ReadJwtToken(raw);
            return token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
