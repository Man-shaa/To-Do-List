using System.Text;
using System.Text.Json;
using Application.Todos.Commands.CreateTodo;
using Application.Todos.DTOs;
using MediatR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using IServiceProvider = System.IServiceProvider;

namespace Presentation.Endpoints.RabbitMq;

internal sealed class TodoCreateConsumerHostedService(
    IServiceProvider serviceProvider,
    ILogger<TodoCreateConsumerHostedService> logger,
    IOptions<RabbitMqOptions> options)
    : BackgroundService
{
    private const string ExchangeName = "todo.exchange";
    private const string QueueName = "todo.create.queue";
    private const string RoutingKey = "todo.create";
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = new()
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        await factory.CreateConnectionAsync(cancellationToken);
        IConnection connection = await factory.CreateConnectionAsync(cancellationToken);
        IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            ExchangeName,
            ExchangeType.Topic,
            true,
            false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            QueueName,
            true,
            false,
            false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            QueueName,
            ExchangeName,
            RoutingKey,
            cancellationToken: cancellationToken);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await channel.BasicConsumeAsync(
            QueueName,
            false,
            consumer,
            cancellationToken);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        IChannel channel = ((AsyncEventingBasicConsumer)sender).Channel;
        try
        {
            string json = Encoding.UTF8.GetString(ea.Body.ToArray());
            TodoCreateDto? message = JsonSerializer.Deserialize<TodoCreateDto>(json);

            if (message is null)
            {
                logger.LogWarning("Received null or invalid TodoCreateMessage.");
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            using IServiceScope scope = serviceProvider.CreateScope();
            IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            TodoCreateDto dto = new()
            {
                Title = message.Title,
                Order = message.Order
            };

            await mediator.Send(new CreateTodoCommand(dto));

            await channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling TodoCreateMessage.");
            await channel.BasicAckAsync(ea.DeliveryTag, false, CancellationToken.None);
        }
    }
}
