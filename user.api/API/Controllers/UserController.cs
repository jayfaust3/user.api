using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Utilities;
using Common.Models.API;
using Common.Models.DTO;
using Common.Models.Data;
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

    [HttpPost]
    public async Task<ActionResult<APIResponse<UserDTO?>>?> Post([FromBody] UserCreateRequest payload, CancellationToken cancellationToken)
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

    [HttpGet("{id}")]
    public async Task<ActionResult<APIResponse<UserDTO?>>?> Get(Guid id, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _userCrudService.GetByIdAsync(id),
            GetMethod,
            cancellationToken
        );

    }

    [HttpGet]
    public async Task<ActionResult<APIResponse<IEnumerable<UserDTO>?>>?> GetAll([Required][FromQuery] string pageToken, CancellationToken cancellationToken)
    {
        return await RunAsyncServiceCall
        (
            async () => await _userCrudService.GetAllAsync(pageToken),
            GetMethod,
            cancellationToken
        );

    }
}
