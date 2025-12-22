using MediatR;

namespace Application.Todos.Commands.DeleteTodo;

public sealed record DeleteAllTodoCommand : IRequest;

public sealed class DeleteAllTodoHandler(ITodoRepository todoDbContext) : IRequestHandler<DeleteAllTodoCommand>
{
    public async Task Handle(DeleteAllTodoCommand request, CancellationToken cancellationToken)
    {
        await todoDbContext.DeleteAllAsync(cancellationToken);
    }
}
