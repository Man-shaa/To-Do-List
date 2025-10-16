using Application.Todos.Commands.UpdateTodo;
using Domain.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace SnapshotTests.Application.Todos.Commands.UpdateTodo;

public class UpdateTodoValidationSnapshotTests
{
    [Fact]
    public Task Invalid_patchDocument_produces_expected_validation_errors()
    {
        var todo = new Todo(1, "Title", new Uri("https://localhost:7214/todos/1"), 1);

        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("add", "/title", from: null, value: "X"));
        patch.Operations.Add(new Operation<Todo>("replace", "", from: null, value: "Y"));
        patch.Operations.Add(new Operation<Todo>("replace", "/title", from: null, value: null));

        var command = new UpdateTodoCommand(todo, patch);
        var validator = new UpdateTodoCommandValidator();

        var result = validator.Validate(command);

        var errors = result.Errors
            .Select(e => new { e.PropertyName, e.ErrorMessage })
            .OrderBy(e => e.PropertyName)
            .ThenBy(e => e.ErrorMessage);

        return Verify(errors);
    }
}
