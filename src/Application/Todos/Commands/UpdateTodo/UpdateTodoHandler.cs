using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using ToDo.Domain.Entities;
using ToDo.Infrastructure.Repositories;

namespace ToDo.Application.Todos.Commands.UpdateTodo;

public record UpdateTodoCommand(int Id, JsonPatchDocument<Todo> PatchDocument) : IRequest<IResult>;

public sealed class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, IResult>
{
    private readonly TodoService _todoService;

    public UpdateTodoHandler(TodoService todoService) => _todoService = todoService;

    public Task<IResult> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = _todoService.GetById(request.Id);

        if (todo is null)
            return Task.FromResult(Results.NotFound(new { error = $"Todo {request.Id} not found" }));

        var isValidOperation = request.PatchDocument.Operations
            .Where(op => !string.Equals(op.op, "replace", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (isValidOperation.Count > 0)
        {
            return Task.FromResult<IResult>(Results.BadRequest(new
            {
                error = $"Only 'replace' operations are allowed",
                invalidOperations = isValidOperation.ToList()
            }));
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