using Application.Todos.Commands.CreateTodo;
using FluentValidation.TestHelper;
using Infrastructure.Repositories.DTOs;

namespace UnitTests.Application.Todos.Commands.CreateTodo;

public sealed class CreateTodoCommandValidatorTests
{
    private static CreateTodoCommand CreateCommand(string? title, int? order = null) =>
        new(new TodoCreateDto { Title = title, Order = order });

    [Fact]
    public void Should_have_error_when_title_is_null()
    {
        var validator = new CreateTodoCommandValidator();
        var result = validator.TestValidate(CreateCommand(null));

        result.ShouldHaveValidationErrorFor(x => x.Todo.Title)
              .WithErrorMessage("Title field required");
    }

    [Fact]
    public void Should_have_error_when_title_is_empty()
    {
        var validator = new CreateTodoCommandValidator();
        var result = validator.TestValidate(CreateCommand(""));

        result.ShouldHaveValidationErrorFor(x => x.Todo.Title)
              .WithErrorMessage("Title must not be empty");
    }

    [Fact]
    public void Should_have_error_when_order_is_negative()
    {
        var validator = new CreateTodoCommandValidator();
        var result = validator.TestValidate(CreateCommand("title", -1));

        result.ShouldHaveValidationErrorFor(x => x.Todo.Order)
              .WithErrorMessage("Order must be positive when provided");
    }

    [Fact]
    public void Should_pass_when_title_and_order_are_valid()
    {
        var validator = new CreateTodoCommandValidator();
        var result = validator.TestValidate(CreateCommand("Valid title", 2));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
