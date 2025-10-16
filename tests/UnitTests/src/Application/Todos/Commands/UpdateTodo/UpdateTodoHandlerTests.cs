using Application.Todos.Commands.UpdateTodo;
using Domain.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace UnitTests.Application.Todos.Commands.UpdateTodo
{
    public sealed class UpdateTodoHandlerTests
    {
        private static Todo MakeTodo(int id = 1, string title = "Title", string url = "https://localhost/todos/1", int order = 1)
            => new Todo(id: id, title: title, url: new Uri(url), order: order);

        [Fact]
        public async Task Handle_should_apply_patch_and_return_same_instance()
        {
            var todo = MakeTodo();
            var patch = new JsonPatchDocument<Todo>();
            patch.Replace(t => t.Title, "Updated Title");

            var handler = new UpdateTodoHandler();
            var result = await handler.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None);

            Assert.Same(todo, result);
            Assert.Equal("Updated Title", result.Title);
        }

        [Fact]
        public async Task Handle_should_apply_multiple_operations()
        {
            var todo = MakeTodo(order: 3);
            var patch = new JsonPatchDocument<Todo>();
            patch.Replace(t => t.Title, "New Title");
            patch.Replace(t => t.Order, 7);

            var handler = new UpdateTodoHandler();
            var result = await handler.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None);

            Assert.Equal("New Title", result.Title);
            Assert.Equal(7, result.Order);
        }

        [Fact]
        public async Task Handle_should_be_noop_with_empty_patch()
        {
            var todo = MakeTodo(title: "Original", order: 5);
            var patch = new JsonPatchDocument<Todo>();

            var handler = new UpdateTodoHandler();
            var result = await handler.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None);

            Assert.Equal("Original", result.Title);
            Assert.Equal(5, result.Order);
            Assert.Same(todo, result);
        }

        [Fact]
        public async Task Handle_should_throw_when_target_path_does_not_exist()
        {
            var todo = MakeTodo();
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", "/DoesNotExist", from: null, value: "X"));

            var handler = new UpdateTodoHandler();

            await Assert.ThrowsAsync<JsonPatchException>(() =>
                handler.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_should_throw_when_value_type_invalid_for_order()
        {
            var todo = MakeTodo(order: 1);
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", "/Order", from: null, value: "not-a-number"));

            var handler = new UpdateTodoHandler();

            await Assert.ThrowsAsync<JsonPatchException>(() =>
                handler.Handle(new UpdateTodoCommand(todo, patch), CancellationToken.None));
        }
    }
}
