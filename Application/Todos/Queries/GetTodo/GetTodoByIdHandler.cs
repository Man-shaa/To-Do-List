using MediatR;
using Microsoft.AspNetCore.Http;
using Todo.Infrastructure.Repositories;

namespace Todo.Application.Todos.Queries.GetTodo;

public record GetTodoByIdCommand(int Id) : IRequest<IResult>;

public sealed class GetTodoByIdHandler(TodoService todoService) : IRequestHandler<GetTodoByIdCommand, IResult>
{
    public Task<IResult> Handle(GetTodoByIdCommand request, CancellationToken cancellationToken)
    {
        var todo = todoService.GetById(request.Id);
        if (todo is null)
            return Task.FromResult(Results.NotFound(new { error = $"Todo {request.Id} not found" }));

        return Task.FromResult(Results.Ok(todo));
    }
}
