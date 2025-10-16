using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Moq;

namespace SnapshotTests.Application.Todos.Queries.GetTodo;

public class GetTodoByIdHandlerSnapshotTests
{
    [Fact]
    public async Task GetTodoByIdHandler_WithExistingTodo_ReturnsTodo()
    {
        var todoServiceMock = new Mock<ITodoService>();
        var getTodoByIdQuery = new GetTodoByIdQuery(1);
        var getTodoByIdHandler = new GetTodoByIdHandler(todoServiceMock.Object);

        todoServiceMock.Setup(t => t.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Todo(1, "Snapshot Title 1", new Uri("https://localhost:7214/todos/1"), 1));

        var sut = getTodoByIdHandler.Handle(getTodoByIdQuery, CancellationToken.None); 

        await Verify(sut);
    }

    [Fact]
    public async Task GetTodoByIdHandler_TodoNotFound_ReturnsNull()
    {
        var todoServiceMock = new Mock<ITodoService>();
        var getTodoByIdQuery = new GetTodoByIdQuery(1);
        var getTodoByIdHandler = new GetTodoByIdHandler(todoServiceMock.Object);

        todoServiceMock.Setup(t => t.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var sut = getTodoByIdHandler.Handle(getTodoByIdQuery, CancellationToken.None);

        await Verify(sut);
    }
}
