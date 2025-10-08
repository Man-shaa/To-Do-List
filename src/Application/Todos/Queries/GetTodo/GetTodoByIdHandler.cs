using Domain.Entities;
using Infrastructure.Repositories;
using MediatR;

namespace Application.Todos.Queries.GetTodo;

public record GetTodoByIdCommand(int Id) : IRequest<Todo?>;

public sealed class GetTodoByIdHandler(ITodoService todoService) : IRequestHandler<GetTodoByIdCommand, Todo?>
{
    public async Task<Todo?> Handle(GetTodoByIdCommand request, CancellationToken cancellationToken) => 
        await todoService.GetByIdAsync(request.Id, cancellationToken);
}
