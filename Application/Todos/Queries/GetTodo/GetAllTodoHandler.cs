using MediatR;
using Microsoft.AspNetCore.Http;
using Todo.Infrastructure.Repositories;

namespace Todo.Application.Todos.Queries.GetTodo;

public record GetAllTodoCommand() : IRequest<IResult>;

public sealed class GetAllTodoHandler(TodoService todoService) : IRequestHandler<GetAllTodoCommand, IResult>
{
    public Task<IResult> Handle(GetAllTodoCommand request, CancellationToken cancellationToken)
    {
        var todos = todoService.GetAll();

        IEnumerable<Domain.Entities.Todo> enumerable = todos.ToList();
        if (enumerable.ToArray().Length == 0)
            return Task.FromResult(Results.NoContent());

        return Task.FromResult(Results.Ok(enumerable));
    }
}
