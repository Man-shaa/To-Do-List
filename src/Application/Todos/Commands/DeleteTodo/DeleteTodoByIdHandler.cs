using MediatR;

namespace Application.Todos.Commands.DeleteTodo;

public sealed record DeleteTodoCommand(int Id) : IRequest<bool>;

public sealed class DeleteTodoByIdHandler(ITodoRepository todoDbContext) : IRequestHandler<DeleteTodoCommand, bool>
{
    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoDbContext.GetByIdAsync(request.Id, cancellationToken);

        return await todoDbContext.DeleteByIdAsync(todo, cancellationToken);
        
    }
}
