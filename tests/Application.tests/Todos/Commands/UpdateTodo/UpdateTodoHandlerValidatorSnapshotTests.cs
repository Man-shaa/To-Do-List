using Application.Todos.Commands.UpdateTodo;
using Domain.Entities;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace ApplicationTests.Todos.Commands.UpdateTodo;


public sealed class UpdateTodoHandlerValidatorTests
{
    [Fact]
    public Task Invalid_patchDocument_produces_expected_validation_errors()
    {
        var todo = new Todo(1, "Title", new Uri("https://localhost:7214/todos/1"), 1);

        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("add", "/title", from: null, value: "X"));
        patch.Operations.Add(new Operation<Todo>("replace", "", from: null, value: "Y"));
        patch.Operations.Add(new Operation<Todo>("replace", "/title", from: null, value: null));

        var command = new UpdateTodoCommand(todo, patch);
        var validator = new UpdateTodoCommandValidator();

        var validatorResult = validator.Validate(command);

        var sut = validatorResult.Errors
            .Select(e => new { e.PropertyName, e.ErrorMessage })
            .OrderBy(e => e.PropertyName)
            .ThenBy(e => e.ErrorMessage);

        return Verify(sut);
    }

    private static Todo MakeTodo(int id = 1, string title = "Title", string url = "https://localhost/todos/1",
        int order = 1) =>
        new(id: id, title: title, url: new Uri(url), order: order);

    private static UpdateTodoCommand MakeCommand(JsonPatchDocument<Todo>? patch) => new(MakeTodo(), patch!);

    [Fact]
    public void Should_have_error_when_patchdocument_is_null()
    {
        var validator = new UpdateTodoCommandValidator();

        var result = validator.TestValidate(new UpdateTodoCommand(MakeTodo(), null!));

        result.ShouldHaveValidationErrorFor(x => x.PatchDocument)
            .WithErrorMessage("PatchDocument is required.");
    }

    [Fact]
    public void Should_have_error_when_operation_is_empty()
    {
        var patch = new JsonPatchDocument<Todo>();
        var validator = new UpdateTodoCommandValidator();

        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor(x => x.PatchDocument.Operations)
            .WithErrorMessage("PatchDocument must have at least one operation.");
    }
    
    [Fact]
    public void Should_have_error_when_any_operation_is_not_replace()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("add", "/Title", from: null, value: "X"));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument")
            .WithErrorMessage("Only 'replace' operations are permitted.");
    }

    [Fact]
    public void Should_have_error_when_path_is_empty()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", path: "", from: null, value: "New Title"));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument")
            .WithErrorMessage("Each operation must have a non-empty 'path'.");
    }

    [Fact]
    public void Should_have_error_when_path_is_whitespace_only()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", path: "   ", from: null, value: "New Title"));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument")
            .WithErrorMessage("Each operation must have a non-empty 'path'.");
    }

    [Fact]
    public void Should_have_error_when_path_does_not_start_with_slash()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", path: "Title", from: null, value: "New Title"));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument")
            .WithErrorMessage("Path must start with '/'.");
    }

    [Fact]
    public void Should_have_error_when_replace_value_is_null()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", path: "/Title", from: null, value: null));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument")
            .WithErrorMessage("Value field required.");
    }

    [Fact]
    public void Should_pass_when_replace_title_with_string()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Replace(t => t.Title, "Updated Title");

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_pass_when_replace_order_with_int()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Replace(t => t.Order, 42);

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_have_error_when_target_path_does_not_exist()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", "/DoesNotExist", from: null, value: "X"));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument");
    }

    [Fact]
    public void Should_have_error_when_wrong_value_type_for_order()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", "/Order", from: null, value: "not-a-number"));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument");
    }

    [Fact]
    public void Should_have_error_when_path_has_subsegment()
    {
        var patch = new JsonPatchDocument<Todo>();
        patch.Operations.Add(new Operation<Todo>("replace", "/Title/Sub", from: null, value: "X"));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument");
    }

    [Fact]
    public void Should_aggregate_multiple_errors_from_multiple_invalid_operations()
    {
        var patch = new JsonPatchDocument<Todo>();

        patch.Operations.Add(new Operation<Todo>("add", "/Title", from: null, value: "X"));

        patch.Operations.Add(new Operation<Todo>("replace", path: "", from: null, value: "Y"));

        patch.Operations.Add(new Operation<Todo>("replace", path: "Order", from: null, value: 1));

        patch.Operations.Add(new Operation<Todo>("replace", path: "/Title", from: null, value: null));

        var validator = new UpdateTodoCommandValidator();
        var result = validator.TestValidate(MakeCommand(patch));

        result.ShouldHaveValidationErrorFor("PatchDocument").WithErrorMessage("Only 'replace' operations are permitted.");
        result.ShouldHaveValidationErrorFor("PatchDocument").WithErrorMessage("Each operation must have a non-empty 'path'.");
        result.ShouldHaveValidationErrorFor("PatchDocument").WithErrorMessage("Path must start with '/'.");
        result.ShouldHaveValidationErrorFor("PatchDocument").WithErrorMessage("Value field required.");
    }
}
