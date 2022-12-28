using Common.Models.Context;
using Microsoft.Extensions.Primitives;
using System.Net;

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

        await _next.Invoke(context);
    }

    private static void SetResponseStatausAsUnauthorized(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    }
}
