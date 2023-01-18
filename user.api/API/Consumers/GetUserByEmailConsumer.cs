using Application.Services.Crud;
using Common.Models.DTO;
using Common.Models.Message;
using Common.Models.Message.Requests;
using MassTransit;

namespace API.Consumers;

public class GetUserByEmailConsumer : BaseConsumer<GetUserByEmailRequest, UserDTO?>
{
    private readonly IUserCrudService _userCrudService;

    public GetUserByEmailConsumer(IUserCrudService userCrudService)
	{
        _userCrudService = userCrudService;
    }

    protected override async Task<UserDTO?> GetResponseAsync(ConsumeContext<IMessage<GetUserByEmailRequest>> context)
    {
        return await _userCrudService.GetByEmailAsync(context.Message.Data.Email);
    }
}

