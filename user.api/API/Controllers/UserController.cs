using Microsoft.AspNetCore.Mvc;
using Common.Models.API;
using Common.Models.DTO;

namespace API.Controllers;

[ApiController]
[Route("users")]
public class UserController : BaseController
{
    public UserController() {}

    [HttpPost]
    public async Task<ActionResult<APIResponse<UserDTO>>> Post([FromBody] UserDTO payload, CancellationToken token)
    {
        return await RunAsyncServiceCall
        (
            async () => await Task.Run(() => payload),
            token,
            HttpMethod.Post.Method
        );

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse<UserDTO>>> Get(string id, CancellationToken token)
    {
        return await RunAsyncServiceCall
        (
            async () => await Task.Run
            (
                () => new UserDTO
                {
                    Id = id,
                    FirstName = "First",
                    LastName = "Last",
                    EmailAddress = "first.last@domain.com"
                }
            ),
            token,
            HttpMethod.Get.Method
        );

    }
}
