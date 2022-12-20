using Common.Models.Message;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Messaging.Service;

public class MessageService : IMessageService
{
    private readonly TimeSpan _requestTimeout = TimeSpan.FromMilliseconds(1);
    private readonly IBusControl _bus;

    public MessageService(IBusControl bus)
    {
        _bus = bus;
    }

    public async Task SendMessage<T>(T message) where T : class, IBaseMessage
    {
        await _bus.Publish(message);
    }

    public async Task<TReturn> SendMessageWithResponse<TSend, TReturn>(TSend message) where TSend : class, IBaseMessage
                                                                                      where TReturn : class, IBaseMessage
    {
        var requestHandler = GetRequestHandler(message);

        Response<TReturn> requestResponse = await requestHandler.GetResponse<TReturn>().ConfigureAwait(true);

        return requestResponse.Message;
    }

    private RequestHandle<TSend> GetRequestHandler<TSend>(TSend message) where TSend : class, IBaseMessage
    {
        IRequestClient<TSend> client = _bus.CreateRequestClient<TSend>(new Uri(""), _requestTimeout);
        return client.Create(message);
    }
}
