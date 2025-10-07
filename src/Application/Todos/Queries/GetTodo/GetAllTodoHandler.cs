using Domain.Entities;
using Infrastructure.Repositories;
using MediatR;

namespace Application.Todos.Queries.GetTodo;

public record GetAllTodoQuery : IRequest<List<Todo>>;

public sealed class GetAllTodoHandler(ITodoService todoService)
    : IRequestHandler<GetAllTodoQuery, List<Todo>>
{
    public async Task<List<Todo>> Handle(GetAllTodoQuery request, CancellationToken cancellationToken)
    {
        var todos = await todoService.GetAllAsync(cancellationToken);

        return await Task.FromResult(todos);
    }
}
