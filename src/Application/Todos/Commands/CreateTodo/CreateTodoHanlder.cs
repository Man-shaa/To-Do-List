using MediatR;
using ToDo.Domain.Entities;
using ToDo.Infrastructure.Repositories;
using ToDo.Application.DTOs;

namespace ToDo.Application.Todos.Commands.CreateTodo;

public record CreateTodoCommand(TodoCreateDto Todo) : IRequest<IResult>;

public sealed class CreateTodoHandler : IRequestHandler<CreateTodoCommand, IResult>
{
    private readonly TodoService _todoService;

    public CreateTodoHandler(TodoService todoService) => _todoService = todoService;

    public Task<IResult> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = _todoService.Create(request.Todo);

        if (todo is null)
            return Task.FromResult(Results.BadRequest(new { error = "Invalid JSON body provided" }));

        return Task.FromResult(Results.Created($"/todos/{todo.Id}", todo));
    }
}