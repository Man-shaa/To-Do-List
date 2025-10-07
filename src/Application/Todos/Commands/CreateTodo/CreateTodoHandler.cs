using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Repositories.DTOs;
using MediatR;

namespace Application.Todos.Commands.CreateTodo;

public record CreateTodoCommand(TodoCreateDto Todo) : IRequest<Todo>;

public sealed class CreateTodoHandler(ITodoService todoService) : IRequestHandler<CreateTodoCommand, Todo>
{
    public async Task<Todo> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        return await todoService.CreateAsync(request.Todo, cancellationToken);
    }
}
