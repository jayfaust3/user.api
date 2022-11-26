using Microsoft.AspNetCore.Mvc;
using Common.Models.API;
using Common.Models.DTO;
using Persistence.Repositories;

namespace API.Controllers;

[ApiController]
[Route("users")]
public class UserController : BaseController
{
    private readonly IUserRepository _repository;

    public UserController(IUserRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse<UserDTO>>> Post([FromBody] UserDTO payload, CancellationToken token)
    {
        return await RunAsyncServiceCall
        (
            async () => await _repository.InsertAsync(payload),
            token,
            PostMethod
        );

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse<UserDTO?>>> Get(Guid id, CancellationToken token)
    {
        return await RunAsyncServiceCall
        (
            async () => await _repository.FindOneAsync(new UserDTO { Id = id }),
            token,
            GetMethod
        );

    }
}
