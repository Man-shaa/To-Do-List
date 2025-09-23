using MediatR;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Application.Todos.Commands.DeleteTodo;

public record DeleteAllTodoCommand() : IRequest<IResult>;

public sealed class DeleteAllTodoHandler : IRequestHandler<DeleteAllTodoCommand, IResult>
{
    private readonly TodoService _todoService;

    public DeleteAllTodoHandler(TodoService todoService)
    {
        _todoService = todoService;
    }

    public Task<IResult> Handle(DeleteAllTodoCommand request, CancellationToken cancellationToken)
    {
        _todoService.DeleteAll();

        return Task.FromResult(Results.NoContent());
    }
}
