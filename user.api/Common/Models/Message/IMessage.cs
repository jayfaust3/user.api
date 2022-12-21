namespace Common.Models.Message;

public interface IMessage<TData>
{
    TData Data { get; }
}
