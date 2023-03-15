using Common.Models.Context;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Application.Services.Crud;
using Common.Models.DTO;

namespace Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
	{
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext, IUserContext userContext)
    {
        var authTokenHeader =
            httpContext.Request.Headers["Authorization"];

        if (StringValues.IsNullOrEmpty(authTokenHeader))
        {
            SetResponseStatausAsUnauthorized(httpContext);
            return;
        }

        var authTokenString = authTokenHeader.ToString();

        var authTokenParts = authTokenString.Split(' ');

        if (authTokenParts.Length != 2)
        {
            SetResponseStatausAsUnauthorized(httpContext);
            return;
        }

        var token = new JwtSecurityToken(authTokenParts[1]);

        bool tokenIsValid = await ValidateAuthTokenAndSetContext(httpContext, token, userContext);

        if (!tokenIsValid)
        {
            SetResponseStatausAsUnauthorized(httpContext);
            return;
        }

        await _next.Invoke(httpContext);
    }

    private async static Task<bool> ValidateAuthTokenAndSetContext(HttpContext httpContext, JwtSecurityToken token, IUserContext userContext)
    {
        var tokenExpiry = GetTokenExpiry(token);
        if (!tokenExpiry.HasValue || tokenExpiry <= DateTimeOffset.Now) return false;

        if (token.Issuer != Environment.GetEnvironmentVariable("AUTH_ISSUER")) return false;

        var tokenAudience = Environment.GetEnvironmentVariable("AUTH_AUDIENCE");
        if (!token.Audiences.Contains(tokenAudience)) return false;

        var emailClaimValue = token.Claims.First(c => c.Type == "email")?.Value;
        if (!string.IsNullOrWhiteSpace(emailClaimValue))
        {
            var userService = httpContext.RequestServices.GetService<IUserCrudService>();

            UserDTO? user = await userService?.GetByEmailAsync(emailClaimValue);

            if (user != null)
            {
                userContext.Id = user?.Id;
            }
            
        }

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
