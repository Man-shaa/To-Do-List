using Application.Todos.Commands.UpdateTodo;
using Domain.Entities;

namespace SnapshotTests.Application.Todos.Commands.UpdateTodo;

public class CreateTodoHandlerSnapshotTests
{
    [Fact]
    public async Task UpdateTodoHandler_WithValidReplaceTitle_ReturnsUpdatedTodo()
    {
        var todo = new Todo(
            1,
            "Initial Title",
            new Uri("https://localhost:7214/todos/1"),
            1);

        var patchDocument = new Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<Todo>();
        patchDocument.Replace(t => t.Title, "Updated Title");

        var updateTodoCommand = new UpdateTodoCommand(todo, patchDocument);
        var updateHandler = new UpdateTodoHandler();

        var sut = await updateHandler.Handle(updateTodoCommand, CancellationToken.None);

        await Verify(sut);
    }
}
