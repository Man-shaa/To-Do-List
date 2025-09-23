using MediatR;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Application.Todos.Queries.GetTodo;

public record GetTodoByIdCommand(int Id) : IRequest<IResult>;

public sealed class GetTodoByIdHandler : IRequestHandler<GetTodoByIdCommand, IResult>
{
    private readonly TodoService _todoService;

    public GetTodoByIdHandler(TodoService todoService)
    {
        _todoService = todoService;
    }

    public Task<IResult> Handle(GetTodoByIdCommand request, CancellationToken cancellationToken)
    {
        var todo = _todoService.GetById(request.Id);
        if (todo is null)
            return Task.FromResult(Results.NotFound(new { error = $"Todo {request.Id} not found" }));

        return Task.FromResult(Results.Ok(todo));
    }
}