namespace API.Consumers;

using System.Text;
using System.Text.Json;
using Common.Models.Message;
using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;

public abstract class BaseConsumer<TSend, TResponse> : IConsumer<IMessage<TSend>>, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public BaseConsumer(IConnectionFactory connectionFactory)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task Consume(ConsumeContext<IMessage<TSend>> context)
    {
        TResponse messageData = await GetResponseAsync(context);

        var responseMessage = new Message<TResponse>(messageData);

        // TODO: Make work
        // await context.RespondAsync<IMessage<TResponse>>(responseMessage);

        RabbitMqReceiveContext receiveContext = context.ReceiveContext as RabbitMqReceiveContext;

        // var exchange = receiveContext.Exchange;

        IBasicProperties receiveProperties = receiveContext.Properties;
        var exchange = receiveProperties.ReplyTo;
        var routingKey = receiveProperties.ReplyTo;

        IBasicProperties sendProperties = _channel.CreateBasicProperties();
        sendProperties.CorrelationId = receiveProperties.CorrelationId;

        var messageBytes = ConvertToBytes(responseMessage);

        _channel.BasicPublish(exchange: exchange,
                             routingKey: routingKey,
                             basicProperties: sendProperties,
                             body: messageBytes);

    }

    public void Dispose() => _connection.Close();

    protected abstract Task<TResponse> GetResponseAsync(ConsumeContext<IMessage<TSend>> context);

    private static byte[] ConvertToBytes(IMessage<TResponse> message)
    {
        var messageJson = JsonSerializer.Serialize(message);

        return Encoding.UTF8.GetBytes(messageJson);
    }
}
