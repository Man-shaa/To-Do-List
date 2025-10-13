using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace Application.Todos.Commands.UpdateTodo;

public record UpdateTodoCommand(Todo Todo, JsonPatchDocument<Todo> PatchDocument) : IRequest<Todo>;


public sealed class UpdateTodoHandler() : IRequestHandler<UpdateTodoCommand, Todo>
{
    public Task<Todo> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        request.PatchDocument.ApplyTo(request.Todo);

        return Task.FromResult(request.Todo);
    }
}
