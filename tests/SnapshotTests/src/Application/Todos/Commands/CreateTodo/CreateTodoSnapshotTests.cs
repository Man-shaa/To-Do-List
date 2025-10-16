using Application.Todos.Commands.CreateTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Repositories.DTOs;
using Moq;

namespace SnapshotTests.Application.Todos.Commands.CreateTodo;

public class CreateTodoSnapshotTests
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
                        0,
                        "Snapshot Title",
                        new Uri("https://localhost:7214/todos/0"),
                        1));

        var createTodoCommand = new CreateTodoCommand(todoCreateDto);
        var createHandler = new CreateTodoHandler(todoServiceMock.Object);

        var sut = await createHandler.Handle(createTodoCommand, CancellationToken.None);

        await Verify(sut);
    }
    //
    // public Task CreateTodoHandler_WithoutIdAndOrder_ReturnsTodo()
    // {
    //     var todo = new
    //     {
    //         Title = "Snapshot Title",
    //         Url = "https://localhost:7214/todos/0",
    //     };
    //
    //     return Verify(todo);
    // }
}
