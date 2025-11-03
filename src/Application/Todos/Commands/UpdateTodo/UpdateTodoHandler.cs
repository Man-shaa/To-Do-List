using Domain.Entities;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace Application.Todos.Commands.UpdateTodo;

public record UpdateTodoCommand(Todo Todo, JsonPatchDocument<Todo> PatchDocument) : IRequest<Todo>;


public sealed class UpdateTodoHandler(ITodoRepository todoDbContext) : IRequestHandler<UpdateTodoCommand, Todo>
{
    public async Task<Todo> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        request.PatchDocument.ApplyTo(request.Todo);
        var updatedTodo = await todoDbContext.UpdateAsync(request.Todo, cancellationToken);

        return updatedTodo;
    }
}
