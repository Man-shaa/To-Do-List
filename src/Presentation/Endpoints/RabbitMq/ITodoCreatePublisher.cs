using Application.Todos.DTOs;

namespace Presentation.Endpoints.RabbitMq;

public interface ITodoCreatePublisher
{
    Task PublishAsync(TodoCreateDto dto, CancellationToken cancellationToken = default);
}
