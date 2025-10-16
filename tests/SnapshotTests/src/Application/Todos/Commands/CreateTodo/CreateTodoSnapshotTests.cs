using Application.Todos.Commands.CreateTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Repositories.DTOs;
using NSubstitute;

namespace SnapshotTests.Application.Todos.Commands.CreateTodo;

public class CreateTodoHandlerSnapshotTests
{
    [Fact]
    public async Task CreateTodoHandler_WithExplicitIdAndOrder_ReturnsTodo()
    {
        var todoServiceMock = Substitute.For<ITodoService>();
        var todoCreateDto = new TodoCreateDto
        {
            Title = "Snapshot Title",
            Order = 1
        };
        
        todoServiceMock.CreateAsync(todoCreateDto, Arg.Any<CancellationToken>())
            .Returns(new Todo(
                1,
                todoCreateDto.Title,
                new Uri("https://localhost:7214/todos/1"),
                todoCreateDto.Order ?? 1));

        var createTodoCommand = new CreateTodoCommand(todoCreateDto);
        var createHandler = new CreateTodoHandler(todoServiceMock);
        
        var sut = await createHandler.Handle(createTodoCommand, CancellationToken.None);
        
        await Verify(sut);
    }
}
