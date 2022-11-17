using System;
using Microsoft.AspNetCore.Mvc;
using Common.Models.API;
using System.Net.Http;

namespace API.Controllers;

[ApiController]
[Route("users")]
public class UserController : BaseController
{
    public UserController()
    {
        
    }

    [HttpPost(Name = "Create User")]
    public async Task<ActionResult<APIResponse<object>>> GetAll(CancellationToken token)
    {
        return await RunAsyncServiceCall
            (
                async () => await Task.Run(new Object()),
                token,
                HttpMethod.Post.Method
            );

    }
}
