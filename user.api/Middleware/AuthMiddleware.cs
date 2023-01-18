using Common.Models.Context;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
	{
        _next = next;
    }

    public async Task Invoke(HttpContext context, IUserContext userContext)
    {
        var authTokenHeader =
            context.Request.Headers["Authorization"];

        if (StringValues.IsNullOrEmpty(authTokenHeader))
        {
            SetResponseStatausAsUnauthorized(context);
            return;
        }

        var authTokenString = authTokenHeader.ToString();

        var authTokenParts = authTokenString.Split(' ');

        if (authTokenParts.Length != 2)
        {
            SetResponseStatausAsUnauthorized(context);
            return;
        }

        var token = new JwtSecurityToken(authTokenParts[1]);

        bool tokenIsValid = ValidateAuthTokenAndSetContext(token, userContext);

        if (!tokenIsValid)
        {
            SetResponseStatausAsUnauthorized(context);
            return;
        }

        await _next.Invoke(context);
    }

    private static bool ValidateAuthTokenAndSetContext(JwtSecurityToken token, IUserContext userContext)
    {
        var tokenExpiry = GetTokenExpiry(token);
        if (!tokenExpiry.HasValue || tokenExpiry <= DateTimeOffset.Now) return false;

        if (token.Issuer != Environment.GetEnvironmentVariable("AUTH_ISSUER")) return false;

        var tokenAudience = Environment.GetEnvironmentVariable("AUTH_AUDIENCE");
        if (!token.Audiences.Contains(tokenAudience)) return false;

        return true;
    }

    private static DateTimeOffset? GetTokenExpiry(JwtSecurityToken token)
    {
        DateTimeOffset? expiryDate = null;

        var expiryString = token.Claims.First(c => c.Type == "exp").Value;

        if (!string.IsNullOrWhiteSpace(expiryString))
        {
            var isValidEpoch = int.TryParse(expiryString, out int parsedExpiryEpoch);

            if (isValidEpoch)
                expiryDate = DateTimeOffset.FromUnixTimeSeconds(parsedExpiryEpoch);
        }

        return expiryDate;
    }

    private static void SetResponseStatausAsUnauthorized(HttpContext context) =>
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
}
