using Microsoft.AspNetCore.Mvc;
using Common.Models.API;
using Common.Models.DTO;
using Common.Utilities;
using Application.Services.Crud;

namespace API.Controllers;

[ApiController]
[Route("users")]
public class UserController : BaseController
{
    private readonly IUserCrudService _userCrudService;

    public UserController
    (
        ILogger logger,
        IUserCrudService userCrudService
    ) :
    base(logger)
    {
        _userCrudService = userCrudService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _userCrudService.GetByIdAsync(id),
            GetMethod,
            cancellationToken
        );

    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? pageToken, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _userCrudService.GetAllAsync(pageToken ?? PagingUtilities.GetDefaultPageToken()),
            GetMethod,
            cancellationToken
        );

    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UserWriteRequest payload, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _userCrudService.CreateAsync
            (
                new UserDTO
                {
                    FirstName = payload.FirstName,
                    LastName = payload.LastName,
                    EmailAddress = payload.EmailAddress
                }
            ),
            PostMethod,
            cancellationToken
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Post(Guid id, [FromBody] UserWriteRequest payload, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _userCrudService.UpdateAsync
            (
                new UserDTO
                {
                    Id = id,
                    FirstName = payload.FirstName,
                    LastName = payload.LastName,
                    EmailAddress = payload.EmailAddress
                }
            ),
            PutMethod,
            cancellationToken
        );
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall<object>
        (
            async () => {
                await _userCrudService.DeleteByIdAsync(id);
                return null;
            },
            DeleteMethod,
            cancellationToken
        );
    }
}
