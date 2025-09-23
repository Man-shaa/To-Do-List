using MediatR;
using ToDo.Application.Todos.DTOs;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Application.Todos.Commands.CreateTodo;

public record CreateTodoCommand(TodoCreateDto Todo) : IRequest<IResult>;

public sealed class CreateTodoHandler(TodoService todoService) : IRequestHandler<CreateTodoCommand, IResult>
{
    public Task<IResult> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = todoService.Create(request.Todo);

        return Task.FromResult(Results.Created($"/todos/{todo.Id}", todo));
    }
}
