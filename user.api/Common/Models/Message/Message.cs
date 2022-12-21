namespace Common.Models.Message;

public class Message<TData> : IMessage<TData>
{
    public Message(TData data) => Data = data;

    public TData Data { get; }
}
