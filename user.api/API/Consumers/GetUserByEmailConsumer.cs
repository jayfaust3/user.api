using Application.Services.Crud;
using Common.Models.DTO;
using Common.Models.Message;
using Common.Models.Message.Requests;
using MassTransit;
using RabbitMQ.Client;

namespace API.Consumers;

public class GetUserByEmailConsumer : BaseConsumer<GetUserByEmailRequest, UserDTO?>
{
    private readonly IUserCrudService _userCrudService;

    public GetUserByEmailConsumer(IConnectionFactory connectionFactory, IUserCrudService userCrudService) : base(connectionFactory)
	{
        _userCrudService = userCrudService;
    }

    protected override async Task<UserDTO?> GetResponseAsync(ConsumeContext<IMessage<GetUserByEmailRequest>> context)
    {
        return await _userCrudService.GetByEmailAsync(context.Message.Data.Email);
    }
}

