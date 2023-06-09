﻿using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UsersManager.Domain.Services;

namespace UsersManager.Service.HostUtilities;

public class UserAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserService _userRepository;

    public UserAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock, IUserService userRepository)
        : base(options, logger, encoder, clock)
    {
        _userRepository = userRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (authHeader.StartsWith(ServiceAuthentication.AuthType.ToLower(), StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring(ServiceAuthentication.AuthType.Length + 1).Trim();
            var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var credentials = credentialsAsEncodedString.Split(':');
            if (await _userRepository.Authenticate(credentials[0], credentials[1]))
            {
                var claims = new[]
                {
                    new Claim(ServiceAuthentication.NameClaimType, credentials[0]),
                    new Claim(ClaimTypes.Role, ServiceAuthentication.AdminRole)
                };
                var identity = new ClaimsIdentity(claims, ServiceAuthentication.AuthType);
                var claimsPrincipal = new ClaimsPrincipal(identity);
                var result = AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
                return await Task.FromResult(result);
            }
        }

        Response.StatusCode = 401;
        Response.Headers.Add("WWW-Authenticate", "Basic realm=\"your-domain.com\"");
        return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
    }
}