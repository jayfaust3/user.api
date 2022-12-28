using Microsoft.AspNetCore.Mvc;
using Common.Models.API;
using Common.Models.DTO;
using System.ComponentModel.DataAnnotations;
using Persistence.Utilities;
using Common.Models.Data;
using Application.Services.Crud;

namespace API.Controllers;

[ApiController]
[Route("users")]
public class UserController : BaseController
{
    private readonly IUserCrudService _userCrudService;

    public UserController(IUserCrudService userCrudService)
    {
        _userCrudService = userCrudService;
    }

    [HttpPost]
    public async Task<ActionResult<APIResponse<UserDTO>>> Post([FromBody] UserDTO payload, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _userCrudService.CreateAsync(payload),
            cancellationToken,
            PostMethod
        );

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse<UserDTO?>>> Get(Guid id, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _userCrudService.GetByIdAsync(id),
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
                PageToken parsedToken = PagingUtilities.ParsePageToken(pageToken);

                return await _userCrudService.GetAllAsync(parsedToken);
            },
            cancellationToken,
            GetMethod
        );

    }
}
