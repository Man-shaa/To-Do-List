using MediatR;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Application.Todos.Commands.DeleteTodo;

public record DeleteTodoCommand(int Id) : IRequest<IResult>;

public sealed class DeleteTodoHandler : IRequestHandler<DeleteTodoCommand, IResult>
{
    private readonly TodoService _todoService;

    public DeleteTodoHandler(TodoService todoService)
    {
        _todoService = todoService;
    }

    public Task<IResult> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = _todoService.GetById(request.Id);

        if (todo is null)
            return Task.FromResult(Results.NoContent());

        _todoService.DeleteById(todo);
        return Task.FromResult(Results.Ok());
    }
}
