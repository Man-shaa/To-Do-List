using Application.Todos.DTOs;
using Domain.Entities;
using MediatR;

namespace Application.Todos.Commands.CreateTodo;

public sealed record CreateTodoCommand(TodoCreateDto Todo) : IRequest<Todo>;

public sealed class CreateTodoHandler(ITodoRepository todoDbContext) : IRequestHandler<CreateTodoCommand, Todo>
{
    public async Task<Todo> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoDbContext.CreateAsync(request.Todo, cancellationToken);

        return todo;
    }
}
