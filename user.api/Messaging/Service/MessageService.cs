using Common.Models.Message;
using MassTransit;

namespace Messaging.Service;

public class MessageService : IMessageService
{
    private readonly TimeSpan _requestTimeout = TimeSpan.FromMilliseconds(1000 * 60);
    private readonly IBusControl _bus;

    public MessageService(IBusControl bus)
    {
        _bus = bus;
    }

    public async Task SendMessage<T>(IMessage<T> message)
    {
        await _bus.Publish(message);
    }

    public async Task<IMessage<TReturn>> SendMessageWithResponse<TSend, TReturn>(IMessage<TSend> message)
    {
        var requestHandler = GetRequestHandler(message);

        Response<IMessage<TReturn>> requestResponse = await requestHandler.GetResponse<IMessage<TReturn>>();

        return requestResponse.Message;
    }

    private RequestHandle<IMessage<TSend>> GetRequestHandler<TSend>(IMessage<TSend> message)
    {
        IRequestClient<IMessage<TSend>> client = _bus.CreateRequestClient<IMessage<TSend>>(_requestTimeout);
        return client.Create(message);
    }
}
