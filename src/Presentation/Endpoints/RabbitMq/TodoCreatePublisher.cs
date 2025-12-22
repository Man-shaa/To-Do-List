using System.Text;
using System.Text.Json;
using Application.Todos.DTOs;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Presentation.Endpoints.RabbitMq;

public sealed class TodoCreatePublisher : ITodoCreatePublisher, IDisposable
{
    private const string ExchangeName = "todo.exchange";
    private const string RoutingKey = "todo.create";
    private readonly Task<IChannel> _channel;
    private readonly Task<IConnection> _connection;

    public TodoCreatePublisher(IOptions<RabbitMqOptions> options)
    {
        RabbitMqOptions cfg = options.Value;

        ConnectionFactory factory = new()
        {
            HostName = cfg.HostName,
            UserName = cfg.UserName,
            Password = cfg.Password,
            Port = cfg.Port
        };

        _connection = factory.CreateConnectionAsync();
        _channel = _connection.Result.CreateChannelAsync();

        _channel.Result.ExchangeDeclareAsync(
            ExchangeName,
            ExchangeType.Topic,
            true,
            false);
    }

    public void Dispose()
    {
        _channel.Result.Dispose();
        _connection.Result.Dispose();
    }

    public async Task PublishAsync(TodoCreateDto dto, CancellationToken cancellationToken = default)
    {
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));

        BasicProperties props = new()
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.Result.BasicPublishAsync(
            ExchangeName,
            RoutingKey,
            true,
            props,
            body,
            cancellationToken);
    }
}
