using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Todo.Infrastructure.Repositories;

namespace Todo.Application.Todos.Commands.UpdateTodo;

public record UpdateTodoCommand(int Id, JsonPatchDocument<Domain.Entities.Todo> PatchDocument) : IRequest<IResult>;

public sealed class UpdateTodoHandler(TodoService todoService) : IRequestHandler<UpdateTodoCommand, IResult>
{
    public Task<IResult> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = todoService.GetById(request.Id);

        if (todo is null)
            return Task.FromResult(Results.NotFound(new { error = $"Todo {request.Id} not found" }));

        var isValidOperation = request.PatchDocument.Operations
            .Where(op => !string.Equals(op.op, "replace", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (isValidOperation.Count > 0)
        {
            return Task.FromResult(Results.BadRequest(
                (error: "Only 'replace' operations are allowed",
                invalidOperations: isValidOperation.ToList()
                )));
        }

        var errors = new List<string>();

        request.PatchDocument.ApplyTo(todo, error =>
            errors.Add(error.ErrorMessage)
        );

        if (errors.Count > 0)
            return Task.FromResult(Results.BadRequest(new { errors }));

        return Task.FromResult(Results.Ok(todo));
    }
}
