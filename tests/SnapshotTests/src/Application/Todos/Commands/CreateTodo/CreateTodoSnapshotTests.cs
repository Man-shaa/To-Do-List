using Application.Todos.Commands.CreateTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Repositories.DTOs;
using Moq;

namespace SnapshotTests.Application.Todos.Commands.CreateTodo;

public class CreateTodoHandlerSnapshotTests
{
    [Fact]
    public async Task CreateTodoHandler_WithExplicitIdAndOrder_ReturnsTodo()
    {
        var todoServiceMock = new Mock<ITodoService>();
        var todoCreateDto = new TodoCreateDto
        {
            Title = "Snapshot Title",
            Order = 1
        };

        todoServiceMock.Setup(t => t.CreateAsync(
                todoCreateDto,
                It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Todo(
                        1,
                        "Snapshot Title",
                        new Uri("https://localhost:7214/todos/1"),
                        1));

        var createTodoCommand = new CreateTodoCommand(todoCreateDto);
        var createHandler = new CreateTodoHandler(todoServiceMock.Object);

        var sut = await createHandler.Handle(createTodoCommand, CancellationToken.None);

        await Verify(sut);
    }
}
