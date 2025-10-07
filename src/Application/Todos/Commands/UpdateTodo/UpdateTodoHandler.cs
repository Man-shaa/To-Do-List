using Domain.Entities;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Application.Todos.Commands.UpdateTodo;

public record UpdateTodoCommand(int Id, JsonPatchDocument<Todo> PatchDocument) : IRequest<Response>;

public record Response(Todo? Todo, List<string>? Errors);

public sealed class UpdateTodoHandler(ITodoService todoService) : IRequestHandler<UpdateTodoCommand, Response>
{
    public async Task<Response> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todoService.GetByIdAsync(request.Id, cancellationToken);

        if (todo is null)
            return new Response(null, null);

        var invalidOperations = request.PatchDocument.Operations
            .Where(op => op.OperationType != OperationType.Replace)
            .ToList();

        if (invalidOperations.Count > 0)
        {
            var errors = invalidOperations
                .Select(op => $"Operation '{op.op}' on path '{op.path}' is not allowed. Only 'replace' is permitted.")
                .ToList();

            return new Response(todo, errors);
        }

        var applyPatchErrors = new List<string>();

        request.PatchDocument.ApplyTo(todo, error => applyPatchErrors.Add(error.ErrorMessage));

        if (applyPatchErrors.Count > 0)
            return new Response(todo, applyPatchErrors);

        return new Response(todo, new List<string>());
    }
}
