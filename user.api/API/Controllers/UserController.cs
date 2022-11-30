using Microsoft.AspNetCore.Mvc;
using Common.Models.API;
using Common.Models.DTO;
using Persistence.Repositories;
using System.ComponentModel.DataAnnotations;
using Persistence.Utilities;
using Common.Models.Data;

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
    public async Task<ActionResult<APIResponse<UserDTO>>> Post([FromBody] UserDTO payload, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _repository.InsertAsync(payload),
            cancellationToken,
            PostMethod
        );

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse<UserDTO?>>> Get(Guid id, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _repository.FindOneAsync(new UserDTO { Id = id }),
            cancellationToken,
            GetMethod
        );

    }

    [HttpGet]
    public async Task<ActionResult<APIResponse<IEnumerable<UserDTO>>>> GetAll([Required][FromQuery] string pageToken, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => {
                PageToken<UserDTO> parsedToken = PagingUtilities.ParsePageToken<UserDTO>(pageToken);

                return await _repository.FindAllAsync(parsedToken);
            },
            cancellationToken,
            GetMethod
        );

    }
}
