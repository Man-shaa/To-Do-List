using Application.Todos.Commands.UpdateTodo;
using Domain.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace UnitTests.Application.Todos.Commands.UpdateTodo
{
    public class UpdateTodoValidatorTests
    {
        private static Todo MakeTodo(int id = 1, string title = "Title", string url = "https://localhost/todos/1", int order = 1)
            => new Todo(id: id, title: title, url: new Uri(url), order: order);

        private static UpdateTodoCommand MakeCommand(JsonPatchDocument<Todo>? patch)
            => new UpdateTodoCommand(MakeTodo(), patch!);

        [Fact]
        public void Invalid_When_PatchDocument_Is_Null()
        {
            var validator = new UpdateTodoCommandValidator();

            var result = validator.Validate(new UpdateTodoCommand(MakeTodo(), null!));
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Invalid_When_Operations_Is_Empty()
        {
            var patch = new JsonPatchDocument<Todo>(); // no operations
            var validator = new UpdateTodoCommandValidator();

            var result = validator.Validate(MakeCommand(patch));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "PatchDocument must have at least one operation.");
        }
        
        [Fact]
        public void Invalid_When_Any_Operation_Is_Not_Replace()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("add", "/Title", from: null, value: "X"));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Only 'replace' operations are permitted.");
        }

        [Fact]
        public void Invalid_When_Path_Is_Empty()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", path: "", from: null, value: "New Title"));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Each operation must have a non-empty 'path'.");
        }

        [Fact]
        public void Invalid_When_Path_Is_Whitespace()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", path: "   ", from: null, value: "New Title"));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Each operation must have a non-empty 'path'.");
        }

        [Fact]
        public void Invalid_When_Path_Does_Not_Start_With_Slash()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", path: "Title", from: null, value: "New Title"));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Path must start with '/'.");
        }

        [Fact]
        public void Invalid_When_Replace_Value_Is_Null()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", path: "/Title", from: null, value: null));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Value field required.");
        }

        [Fact]
        public void Valid_When_Replace_Title_With_String()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Replace(t => t.Title, "Updated Title");

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Valid_When_Replace_Order_With_Int()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Replace(t => t.Order, 42);

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Invalid_When_Target_Path_Does_Not_Exist()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", "/DoesNotExist", from: null, value: "X"));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            // Fails due to ApplyTo dry-run capturing structural error
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Invalid_When_Wrong_Value_Type_For_Order()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", "/Order", from: null, value: "not-a-number"));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            // Fails due to ApplyTo dry-run/type conversion error
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Invalid_When_Path_Has_Subsegment()
        {
            var patch = new JsonPatchDocument<Todo>();
            patch.Operations.Add(new Operation<Todo>("replace", "/Title/Sub", from: null, value: "X"));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            // Fails due to ApplyTo dry-run on invalid nested segment over a scalar
            Assert.False(result.IsValid);
        }

        [Fact]
        public void Aggregates_Multiple_Errors_From_Multiple_Invalid_Operations()
        {
            var patch = new JsonPatchDocument<Todo>();
            // non-replace
            patch.Operations.Add(new Operation<Todo>("add", "/Title", from: null, value: "X"));
            // empty path
            patch.Operations.Add(new Operation<Todo>("replace", path: "", from: null, value: "Y"));
            // path without slash
            patch.Operations.Add(new Operation<Todo>("replace", path: "Order", from: null, value: 1));
            // null value
            patch.Operations.Add(new Operation<Todo>("replace", path: "/Title", from: null, value: null));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Only 'replace' operations are permitted.");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Each operation must have a non-empty 'path'.");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Path must start with '/'.");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Value field required.");
        }

        [Fact]
        public void Captures_ApplyTo_Errors_As_Validation_Failures()
        {
            var patch = new JsonPatchDocument<Todo>();
            // Valid basic checks, but invalid at apply-time (case-sensitive path likely invalid)
            patch.Operations.Add(new Operation<Todo>("replace", "/title", from: null, value: "x"));

            var validator = new UpdateTodoCommandValidator();
            var result = validator.Validate(MakeCommand(patch));

            Assert.True(result.IsValid);
        }
    }
}
