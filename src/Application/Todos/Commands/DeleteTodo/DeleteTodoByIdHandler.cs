using Infrastructure.Repositories;
using MediatR;

namespace Application.Todos.Commands.DeleteTodo;

public record DeleteTodoCommand(int Id) : IRequest<bool>;

public sealed class DeleteTodoByIdHandler(ITodoService todoService) : IRequestHandler<DeleteTodoCommand, bool>
{
    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoService.GetByIdAsync(request.Id, cancellationToken);

        return await todoService.DeleteByIdAsync(todo, cancellationToken);
        
    }
}
