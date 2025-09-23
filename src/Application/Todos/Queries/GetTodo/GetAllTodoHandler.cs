using MediatR;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Application.Todos.Queries.GetTodo;

public record GetAllTodoCommand() : IRequest<IResult>;

public sealed class GetAllTodoHandler : IRequestHandler<GetAllTodoCommand, IResult>
{
    private readonly TodoService _todoService;

    public GetAllTodoHandler(TodoService todoService)
    {
        _todoService = todoService;
    }

    public Task<IResult> Handle(GetAllTodoCommand request, CancellationToken cancellationToken)
    {
        var todos = _todoService.GetAll();

        if (todos.ToArray().Length == 0)
            return Task.FromResult(Results.NoContent());

        return Task.FromResult(Results.Ok(todos));
    }
}