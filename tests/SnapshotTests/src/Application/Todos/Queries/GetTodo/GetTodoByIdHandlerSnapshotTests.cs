using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using NSubstitute;

namespace SnapshotTests.Application.Todos.Queries.GetTodo;

public sealed class GetTodoByIdHandlerSnapshotTests
{
    [Fact]
    public async Task GetTodoByIdHandler_WithExistingTodo_ReturnsTodo()
    {
        var todoServiceMock = Substitute.For<ITodoService>();
        var getTodoByIdQuery = new GetTodoByIdQuery(1);

        todoServiceMock.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Todo(
                id: 1,
                title: "Snapshot Title 1",
                url: new Uri("https://localhost:7214/todos/1"),
                order: 1));

        var getTodoByIdHandler = new GetTodoByIdHandler(todoServiceMock);
        var sut = getTodoByIdHandler.Handle(getTodoByIdQuery, CancellationToken.None); 

        await Verify(sut);
    }

    [Fact]
    public async Task GetTodoByIdHandler_TodoNotFound_ReturnsNull()
    {
        var todoServiceMock = Substitute.For<ITodoService>();
        var getTodoByIdQuery = new GetTodoByIdQuery(1);
        var getTodoByIdHandler = new GetTodoByIdHandler(todoServiceMock);

        todoServiceMock.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns((Todo?)null);

        var sut = getTodoByIdHandler.Handle(getTodoByIdQuery, CancellationToken.None);

        await Verify(sut);
    }
}
