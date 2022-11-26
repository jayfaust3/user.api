using System;
using Microsoft.AspNetCore.Mvc;
using Common.Models.API;
using System.Net.Http;

namespace API.Controllers;

public abstract class BaseController : ControllerBase
{
    protected static string GetMethod = HttpMethod.Get.Method;

    protected static string PostMethod = HttpMethod.Post.Method;

    private readonly int _serviceCallRetryCount =
        int.Parse(Environment.GetEnvironmentVariable("SERVICE_CALL_RETRY_COUNT") ?? "1");

    protected async Task<ActionResult<APIResponse<TResult>>> RunAsyncServiceCall<TResult>
    (
        Func<Task<TResult>> call,
        CancellationToken token,
        string httpMethod
    )
    {
        ActionResult<APIResponse<TResult>> response = null;

        for (var currentAttempt = 1; currentAttempt <= _serviceCallRetryCount; currentAttempt++)
        {
            try
            {
                TResult serviceResult = await Task.Run(call, token);

                if (httpMethod == PostMethod)
                {
                    response = HandlePostResult(serviceResult);
                }
                else
                {
                    response = HandleNonPostResult(serviceResult);
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

    private static ActionResult<APIResponse<TResult>> HandlePostResult<TResult>(TResult serviceResult)
    {
        return new ObjectResult
        (
            new APIResponse<TResult>(serviceResult)
        )
        {
            StatusCode = 201
        };
    }

    private ActionResult<APIResponse<TResult>> HandleNonPostResult<TResult>(TResult serviceResult)
    {
        return Ok
        (
            new APIResponse<TResult>(serviceResult)
        );
    }

    private static ActionResult<APIResponse<TResult>> HandleFailureResult<TResult>(Exception exception)
    {
        return new ObjectResult
        (
            new APIResponse<TResult>
            (
                (TResult)((object)null),
                exception.Message
            )
        )
        {
            StatusCode = 500
        };
    }
}

