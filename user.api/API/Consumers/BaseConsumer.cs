namespace API.Consumers;

using Common.Models.Message;
using MassTransit;

public abstract class BaseConsumer<TSend, TResponse> : IConsumer<IMessage<TSend>>
{
    public async Task Consume(ConsumeContext<IMessage<TSend>> context)
    {
        TResponse response = await GetResponseAsync(context);
        await context.RespondAsync<IMessage<TResponse>>(new Message<TResponse>(response));
    }

    protected abstract Task<TResponse> GetResponseAsync(ConsumeContext<IMessage<TSend>> context);
}
