using Common.Models.Message;

namespace Messaging.Service
{
    public interface IMessageService
    {
        Task SendMessage<T>(T message) where T : class, IBaseMessage;
        Task<TReturn> SendMessageWithResponse<TSend, TReturn>(TSend message)
            where TSend : class, IBaseMessage
            where TReturn : class, IBaseMessage;
    }
}