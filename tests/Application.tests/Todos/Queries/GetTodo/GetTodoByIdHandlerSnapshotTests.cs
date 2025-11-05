using Application.Todos.Queries.GetTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Moq;
using NSubstitute;

namespace Application.Tests.Todos.Queries.GetTodo;

public sealed class GetTodoByIdQueryTests
{
    [Fact]
    public async Task GetTodoByIdHandler_WithExistingTodo_ReturnsTodo()
    {
        var todoDbContext = Substitute.For<ITodoRepository>();
        var getTodoByIdQuery = new GetTodoByIdQuery(1);

        todoDbContext.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Todo(
                id: 1,
                title: "Snapshot Title 1",
                url: new Uri("https://localhost:7214/todos/1"),
                order: 1));

        var getTodoByIdHandler = new GetTodoByIdHandler(todoDbContext);
        var sut = getTodoByIdHandler.Handle(getTodoByIdQuery, CancellationToken.None); 

        await Verify(sut);
    }

    [Fact]
    public async Task GetTodoByIdHandler_TodoNotFound_ReturnsNull()
    {
        var todoDbContext = Substitute.For<ITodoRepository>();
        var getTodoByIdQuery = new GetTodoByIdQuery(1);
        var getTodoByIdHandler = new GetTodoByIdHandler(todoDbContext);

        todoDbContext.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns((Todo?)null);

        var sut = getTodoByIdHandler.Handle(getTodoByIdQuery, CancellationToken.None);

        await Verify(sut);
    }

    [Fact]
    public async Task Handle_should_return_todo_by_id_from_service()
    {
        var ct = CancellationToken.None;
        const int id = 42;
        var expected = new Todo(id, "todo 42", new Uri("http://localhost/todos/42"), 42);

        var todoDbContext = new Mock<ITodoRepository>();
        todoDbContext
            .Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new GetTodoByIdHandler(todoDbContext.Object);

        var result = await handler.Handle(new GetTodoByIdQuery(id), ct);

        Assert.Same(expected, result);
        todoDbContext.Verify(s => s.GetByIdAsync(id, ct), Times.Once);
        todoDbContext.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenTodoNotFound()
    {
        var ct = CancellationToken.None;
        const int id = 7;

        var todoDbContext = new Mock<ITodoRepository>();
        todoDbContext
            .Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var handler = new GetTodoByIdHandler(todoDbContext.Object);

        var result = await handler.Handle(new GetTodoByIdQuery(id), ct);

        Assert.Null(result);
        todoDbContext.Verify(s => s.GetByIdAsync(id, ct), Times.Once);
        todoDbContext.VerifyNoOtherCalls();
    }
}
