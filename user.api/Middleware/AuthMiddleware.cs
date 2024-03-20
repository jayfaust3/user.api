using Microsoft.Extensions.Primitives;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Common.Models.Context;
using Common.Models.DTO;
using Application.Services.Crud;

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
        if (!bool.Parse(Environment.GetEnvironmentVariable("DISABLE_AUTH") ?? "false"))
        {
            var authorizationHeaderValue = httpContext.Request.Headers["Authorization"];

            if (StringValues.IsNullOrEmpty(authorizationHeaderValue))
            {
                SetResponseStatausAsUnauthorized(httpContext);
                return;
            }

            var token = await ExtractTokenFromAuthorizationHeaderValue(authorizationHeaderValue);

            bool tokenIsValid = token is not null ? await ValidateAuthTokenAndSetContext(httpContext, token, userContext) : false;

            if (!tokenIsValid)
            {
                SetResponseStatausAsUnauthorized(httpContext);
                return;
            }
        }

        await _next.Invoke(httpContext);
    }

    private async Task<JwtSecurityToken?> ExtractTokenFromAuthorizationHeaderValue(string authorizationHeaderValue)
    {
        JwtSecurityToken? token = null;

        var authTokenParts = authorizationHeaderValue.Split(' ');

        if (authTokenParts.Length == 2)
            token = new JwtSecurityToken(authTokenParts[1]);

        return token;
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

            if (userService is not null)
            {
                UserDTO? user = await userService.GetByEmailAsync(emailClaimValue);

                if (user is not null)
                {
                    userContext.Id = user.Id;
                }
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
