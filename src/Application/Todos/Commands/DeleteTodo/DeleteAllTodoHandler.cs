using Infrastructure.Repositories;
using MediatR;

namespace Application.Todos.Commands.DeleteTodo;

public record DeleteAllTodoCommand : IRequest;

public sealed class DeleteAllTodoHandler(ITodoService todoService) : IRequestHandler<DeleteAllTodoCommand>
{
    public async Task Handle(DeleteAllTodoCommand request, CancellationToken cancellationToken)
    {
        await todoService.DeleteAllAsync(cancellationToken);
    }
}
