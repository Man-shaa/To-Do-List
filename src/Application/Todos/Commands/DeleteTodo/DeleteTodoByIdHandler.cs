using Infrastructure.Repositories;
using MediatR;

namespace Application.Todos.Commands.DeleteTodo;

public record DeleteTodoCommand(int Id) : IRequest;

public sealed class DeleteTodoByIdHandler(ITodoService todoService) : IRequestHandler<DeleteTodoCommand>
{
    public async Task Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoService.GetByIdAsync(request.Id, cancellationToken);

        await todoService.DeleteByIdAsync(todo, cancellationToken);
    }
}
