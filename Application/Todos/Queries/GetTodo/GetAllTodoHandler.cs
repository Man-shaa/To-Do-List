using MediatR;
using Microsoft.AspNetCore.Http;
using ToDo.Domain.Entities;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Application.Todos.Queries.GetTodo;

public record GetAllTodoCommand() : IRequest<IResult>;

public sealed class GetAllTodoHandler(TodoService todoService) : IRequestHandler<GetAllTodoCommand, IResult>
{
    public Task<IResult> Handle(GetAllTodoCommand request, CancellationToken cancellationToken)
    {
        var todos = todoService.GetAll();

        IEnumerable<Todo> enumerable = todos.ToList();
        if (enumerable.ToArray().Length == 0)
            return Task.FromResult(Results.NoContent());

        return Task.FromResult(Results.Ok(enumerable));
    }
}
