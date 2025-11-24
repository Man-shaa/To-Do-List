using Application.Todos.Commands.UpdateTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;

namespace Application.Tests.Todos.Commands.UpdateTodo;

public sealed class UpdateTodoHandlerTests
{
    private static Todo MakeTodo(int id = 1, string title = "Title", string url = "https://localhost/todos/1",
        int order = 1) =>
        new(id: id, title: title, url: new Uri(url), order: order);

    [Fact]
    public async Task UpdateTodoHandler_WithValidReplaceTitle_ReturnsUpdatedTodo()
    {
        var todoDbContextMock = new Mock<ITodoRepository>();
        var todo = MakeTodo();

        todoDbContextMock.Setup(s => s.UpdateAsync(todo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        var patchDocument = new JsonPatchDocument<Todo>();
        patchDocument.Replace(t => t.Title, "Updated Title");

        var updateTodoCommand = new UpdateTodoCommand(todo, patchDocument);
        var updateHandler = new UpdateTodoHandler(todoDbContextMock.Object);

        var sut = await updateHandler.Handle(updateTodoCommand, CancellationToken.None);

        await Verify(sut);
    }
    
    [Fact]
    public async Task Handle_should_apply_patch_and_return_same_instance()
    {
        var todoDbContextMock = new Mock<ITodoRepository>();
        var todo = MakeTodo();

        todoDbContextMock.Setup(s => s.UpdateAsync(todo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);
        var patch = new JsonPatchDocument<Todo>();
        patch.Replace(t => t.Title, "Updated Title");

        var handler = new UpdateTodoHandler(todoDbContextMock.Object);
        var sut = await handler.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None);

        Assert.Same(todo, sut);
        Assert.Equal("Updated Title", sut.Title);
    }

    [Fact]
    public async Task Handle_should_apply_multiple_operations()
    {
        var todoDbContextMock = new Mock<ITodoRepository>();
        var todo = MakeTodo(order: 3);

        todoDbContextMock.Setup(s => s.UpdateAsync(todo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        var patch = new JsonPatchDocument<Todo>();
        patch.Replace(t => t.Title, "New Title");
        patch.Replace(t => t.Order, 7);

        var handler = new UpdateTodoHandler(todoDbContextMock.Object);
        var sut = await handler.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None);

        Assert.Equal("New Title", sut.Title);
        Assert.Equal(7, sut.Order);
    }

    [Fact]
    public async Task Handle_should_be_noop_with_empty_patch()
    {
        var todoDbContextMock = new Mock<ITodoRepository>();
        var todo = MakeTodo(title: "Original", order: 5);

        todoDbContextMock.Setup(s => s.UpdateAsync(todo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);
        var patch = new JsonPatchDocument<Todo>();

        var handler = new UpdateTodoHandler(todoDbContextMock.Object);
        var sut = await handler.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None);

        Assert.Equal("Original", sut.Title);
        Assert.Equal(5, sut.Order);
        Assert.Same(todo, sut);
    }

    [Fact]
    public async Task Handle_should_throw_when_target_path_does_not_exist()
    {
        var todoDbContextMock = new Mock<ITodoRepository>();
        var todo = MakeTodo();

        todoDbContextMock.Setup(s => s.UpdateAsync(todo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", "/DoesNotExist", from: null, value: "X"));

        var sut = new UpdateTodoHandler(todoDbContextMock.Object);

        await Assert.ThrowsAsync<JsonPatchException>(() =>
            sut.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_should_throw_when_value_type_invalid_for_order()
    {
        var todoDbContextMock = new Mock<ITodoRepository>();
        var todo = MakeTodo(order: 1);

        todoDbContextMock.Setup(s => s.UpdateAsync(todo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", "/Order", from: null, value: "not-a-number"));

        var sut = new UpdateTodoHandler(todoDbContextMock.Object);

        await Assert.ThrowsAsync<JsonPatchException>(() =>
            sut.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None));
    }
}
