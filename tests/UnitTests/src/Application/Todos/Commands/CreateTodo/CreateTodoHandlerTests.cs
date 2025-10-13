using Application.Todos.Commands.CreateTodo;
using Domain.Entities;
using Infrastructure.Repositories;
using Infrastructure.Repositories.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace  UnitTests.Application.Todos.Commands.CreateTodo;

public sealed class CreateTodoHandlerTests
{
    [Fact]
    public async Task Handle_should_return_created_result_with_created_todo()
    {
        var dto = new TodoCreateDto { Title = "Test Title", Order = 3 };
        var createdTodo = new Todo(12, dto.Title, new Uri("https://localhost:7214/todos/12"), dto.Order ?? 3);
        var todoServiceMock = new Mock<ITodoService>();

        todoServiceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(createdTodo);

        var createHandler = new CreateTodoHandler(todoServiceMock.Object);
        var createCommand = new CreateTodoCommand(dto);

        var result = await createHandler.Handle(createCommand, CancellationToken.None);

        var createdResult = Assert.IsType<Created<Todo>>(result);
        Assert.Same(createdTodo, createdResult.Value);

        todoServiceMock.Verify(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        todoServiceMock.VerifyNoOtherCalls();
    }
}

