using Microsoft.AspNetCore.Mvc;
using Common.Models.API;

namespace API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected const string GetMethod = "GET";

    protected const string PostMethod = "POST";

    protected const string PutMethod = "PUT";

    protected const string PatchMethod = "PATCH";

    private const int CreatedStatusCode = 201;

    private const int NotFoundStatusCode = 404;

    private const int ServerErrorStatusCode = 500;

    private readonly int _serviceCallRetryCount =
        int.Parse(Environment.GetEnvironmentVariable("SERVICE_CALL_RETRY_COUNT") ?? "1");

    protected async Task<ActionResult<APIResponse<TResult>>> RunAsyncServiceCall<TResult> 
    (
        Func<Task<TResult>> call,
        CancellationToken token,
        string httpMethod
    )
        where TResult : class
    {
        ActionResult<APIResponse<TResult>> response = null;

        for (var currentAttempt = 1; currentAttempt <= _serviceCallRetryCount; currentAttempt++)
        {
            try
            {
                TResult serviceResult = await Task.Run(call, token);

                switch (httpMethod)
                {
                    case GetMethod:
                        if (serviceResult == null) response = HandleNullResult<TResult>();
                        else response = HandleNonPostResult(serviceResult);
                        break;
                    case PostMethod:
                        response = HandlePostResult(serviceResult);
                        break;
                    default:
                        break;
                }

                break;
            }
            catch(Exception ex)
            {
                if (currentAttempt == _serviceCallRetryCount)
                    response = HandleFailureResult<TResult>(ex);
            }
        }

        return response;
    }

    private static ActionResult<APIResponse<TResult>> HandlePostResult<TResult>(TResult serviceResult) where TResult : class
    {
        return new ObjectResult
        (
            new APIResponse<TResult>(serviceResult)
        )
        {
            StatusCode = CreatedStatusCode
        };
    }

    private ActionResult<APIResponse<TResult>> HandleNonPostResult<TResult>(TResult serviceResult) where TResult : class
    {
        return Ok
        (
            new APIResponse<TResult>(serviceResult)
        );
    }

    private static ActionResult<APIResponse<TResult>> HandleNullResult<TResult>() where TResult : class
    {
        return new ObjectResult
        (
            new APIResponse<TResult>
            (
                null,
                "Entity not found"
            )
        )
        {
            StatusCode = NotFoundStatusCode
        };
    }

    private static ActionResult<APIResponse<TResult>> HandleFailureResult<TResult>(Exception exception) where TResult : class
    {
        return new ObjectResult
        (
            new APIResponse<TResult>
            (
                null,
                exception.Message
            )
        )
        {
            StatusCode = ServerErrorStatusCode
        };
    }
}

