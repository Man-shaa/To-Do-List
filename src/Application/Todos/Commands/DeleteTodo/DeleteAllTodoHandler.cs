using MediatR;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Application.Todos.Commands.DeleteTodo;

public record DeleteAllTodoCommand() : IRequest<IResult>;

public sealed class DeleteAllTodoHandler(TodoService todoService) : IRequestHandler<DeleteAllTodoCommand, IResult>
{
    public Task<IResult> Handle(DeleteAllTodoCommand request, CancellationToken cancellationToken)
    {
        todoService.DeleteAll();

        return Task.FromResult(Results.NoContent());
    }
}
