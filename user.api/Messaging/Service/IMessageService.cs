using Common.Models.Message;

namespace Messaging.Service
{
    public interface IMessageService
    {
        Task SendMessage<T>(IMessage<T> message);

        Task<IMessage<TReturn>> SendMessageWithResponse<TSend, TReturn>(IMessage<TSend> message);
    }
}