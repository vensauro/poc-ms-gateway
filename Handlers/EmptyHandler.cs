using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class EmptyHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public EmptyHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                        ILoggerFactory logger,
                        UrlEncoder encoder,
                        ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());
}
