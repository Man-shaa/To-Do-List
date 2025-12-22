using Domain.Entities;
using MediatR;

namespace Application.Todos.Queries.GetTodo;

public sealed record GetTodoByIdQuery(int Id) : IRequest<Todo?>;

public sealed class GetTodoByIdHandler(ITodoRepository todoDbContext) : IRequestHandler<GetTodoByIdQuery, Todo?>
{
    public async Task<Todo?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken) => 
        await todoDbContext.GetByIdAsync(request.Id, cancellationToken);
}
