using Domain.Entities;
using MediatR;

namespace Application.Todos.Queries.GetTodo;

public record GetAllTodoQuery : IRequest<List<Todo>>;

public sealed class GetAllTodoHandler(ITodoRepository todoDbContext)
    : IRequestHandler<GetAllTodoQuery, List<Todo>>
{
    public async Task<List<Todo>> Handle(GetAllTodoQuery request, CancellationToken cancellationToken)
    {
        var todos = await todoDbContext.GetAllAsync(cancellationToken);

        return await Task.FromResult(todos);
    }
}
