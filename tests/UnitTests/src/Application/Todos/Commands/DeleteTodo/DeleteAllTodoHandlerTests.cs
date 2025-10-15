using Application.Todos.Commands.DeleteTodo;
using Infrastructure.Repositories;
using Moq;

namespace UnitTests.Application.Todos.Commands.DeleteTodo
{
    public sealed class DeleteAllTodoHandlerTests
    {
        [Fact]
        public async Task Handle_should_call_DeleteAllAsync_once_with_token_and_no_other_calls()
        {
            var todoServiceMock = new Mock<ITodoService>();
            var cts = new CancellationTokenSource();

            todoServiceMock.Setup(s => s.DeleteAllAsync(It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

            var handler = new DeleteAllTodoHandler(todoServiceMock.Object);

            await handler.Handle(new DeleteAllTodoCommand(), cts.Token);

            todoServiceMock.Verify(s => s.DeleteAllAsync(It.Is<CancellationToken>(t => t == cts.Token)), Times.Once);
            todoServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_should_propagate_exception_from_service()
        {
            var todoServiceMock = new Mock<ITodoService>();

            todoServiceMock.Setup(s => s.DeleteAllAsync(It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new InvalidOperationException("failure"));

            var handler = new DeleteAllTodoHandler(todoServiceMock.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                handler.Handle(new DeleteAllTodoCommand(), CancellationToken.None));

            todoServiceMock.Verify(s => s.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            todoServiceMock.VerifyNoOtherCalls();
        }
    }
}
