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
            PostMethod
        );

    }

    [HttpGet("{token}")]
    public async Task<ActionResult<APIResponse<IEnumerable<UserDTO>>>> Get(Guid searchToken, CancellationToken token)
    {
        return await RunAsyncServiceCall
        (
            async () => await Task.Run
            (
                () => new List<UserDTO>
                {
                    new UserDTO
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "First",
                        LastName = "Last",
                        EmailAddress = "first.last@domain.com"
                    }
                } as IEnumerable<UserDTO>
            ),
            token,
            GetMethod
        );

    }
}
