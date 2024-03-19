using Microsoft.AspNetCore.Mvc;
using Common.Models.API;
using Common.Exceptions;

namespace API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected const string GetMethod = "GET";

    protected const string PostMethod = "POST";

    protected const string PutMethod = "PUT";

    protected const string PatchMethod = "PATCH";

    protected const int CreatedStatusCode = 201;

    protected const int BadRequestStatusCode = 400;

    protected const int NotFoundStatusCode = 404;

    protected const int ConflictStatusCode = 409;

    protected const int ServerErrorStatusCode = 500;

    private readonly int _serviceCallRetryCount;

    private readonly ILogger _logger;

    protected BaseController(ILogger logger)
    {
        _serviceCallRetryCount =
            int.Parse(Environment.GetEnvironmentVariable("SERVICE_CALL_RETRY_COUNT") ?? "1");

        _logger = logger;
    }

    protected async Task<ActionResult<APIResponse<TResult?>>> RunAsyncServiceCall<TResult> 
    (
        Func<Task<TResult?>> call,
        string httpMethod,
        CancellationToken token
    )
        where TResult : class?
    {
        ActionResult<APIResponse<TResult?>> response = null;

        for (var currentAttempt = 1; currentAttempt <= _serviceCallRetryCount; currentAttempt++)
        {
            try
            {
                TResult? serviceResult = await Task.Run(call, token);

                switch (httpMethod)
                {
                    case GetMethod:
                        if (serviceResult == null) response = HandleNullResult<TResult?>();
                        else response = HandleNonPostResult(serviceResult);
                        break;
                    case PostMethod:
                        response = HandlePostResult(serviceResult);
                        break;
                    case PutMethod:
                        response = HandleNonPostResult(serviceResult);
                        break;
                    default:
                        break;
                }

                break;
            }
            catch(Exception ex)
            {
                if (ex is BadRequestException)
                {
                    response = HandleFailureResult<TResult?>(httpMethod, ex, BadRequestStatusCode);
                    break;
                }

                if (ex is NotFoundException )
                {
                    response = HandleFailureResult<TResult?>(httpMethod, ex, NotFoundStatusCode);
                    break;
                }

                if (ex is ConflictException)
                {
                    response = HandleFailureResult<TResult?>(httpMethod, ex, ConflictStatusCode);
                    break;
                }

                if (currentAttempt == _serviceCallRetryCount)
                {
                    response = HandleFailureResult<TResult?>(httpMethod, ex);
                    break;
                }
            }
        }

        return response;
    }

    private static ActionResult<APIResponse<TResult?>> HandlePostResult<TResult>(TResult serviceResult) where TResult : class?
    {
        return new ObjectResult
        (
            new APIResponse<TResult?>(serviceResult)
        )
        {
            StatusCode = CreatedStatusCode
        };
    }

    private ActionResult<APIResponse<TResult?>> HandleNonPostResult<TResult>(TResult serviceResult) where TResult : class?
    {
        return Ok
        (
            new APIResponse<TResult?>(serviceResult)
        );
    }

    private static ActionResult<APIResponse<TResult?>> HandleNullResult<TResult>() where TResult : class?
    {
        return new ObjectResult
        (
            new APIResponse<TResult?>
            (
                null,
                "Entity not found"
            )
        )
        {
            StatusCode = NotFoundStatusCode
        };
    }

    private ActionResult<APIResponse<TResult?>> HandleFailureResult<TResult>(string httpMethod, Exception exception, int statusCode = ServerErrorStatusCode) where TResult : class?
    {
        var errorMessage = exception.Message;

        _logger.LogError($"Error occurred on {httpMethod} request: '{errorMessage}'");

        return new ObjectResult
        (
            new APIResponse<TResult?>
            (
                null,
                errorMessage
            )
        )
        {
            StatusCode = statusCode
        };
    }
}
