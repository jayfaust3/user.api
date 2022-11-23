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
                    response = Created
                    (
                        string.Empty,
                        new APIResponse<TResult>
                        (
                            serviceResult
                            
                        )
                    );
                }
                else
                {
                    response = Ok
                    (
                        new APIResponse<TResult>
                        (
                            serviceResult
                        )
                    );
                }

                return response;
            }
            catch(Exception ex)
            {
                if (currentAttempt == _serviceCallRetryCount)
                    response = new ObjectResult
                    (
                        new APIResponse<TResult>
                        (
                            (TResult)((object)null),
                            ex.Message
                        )
                    )
                    {
                        StatusCode = 500
                    };
            }
        }

        return response;
    }
}

