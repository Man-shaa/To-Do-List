using Application.Todos.Commands.CreateTodo;
using Application.Todos.DTOs;
using FluentValidation.Results;
using FluentValidation.TestHelper;

namespace Application.Tests.Todos.Commands.CreateTodo;

public sealed class CreateTodoCommandValidatorTests
{
    private static ValidationResult Validate(string? title, int? order)
    {
        var command = new CreateTodoCommand(new TodoCreateDto { Title = title, Order = order });
        var validator = new CreateTodoCommandValidator();
        return validator.Validate(command);
    }

    private static object Shape(ValidationResult result) =>
        new
        {
            result.IsValid,
            Errors = result.Errors
                .Select(e => new { e.PropertyName, e.ErrorCode, e.ErrorMessage })
                .OrderBy(e => e.PropertyName)
                .ThenBy(e => e.ErrorCode)
                .ToList()
        };

    public static IEnumerable<object[]> Cases =>
    [
        ["valid_basic", "New Todo", 1],
        ["title_null", null!, 1],
        ["title_empty", "", 1],
        ["title_whitespace", "   ", 1],
        ["order_null", "New Todo", null!],
        ["order_negative", "New Todo", -1]
    ];
    
    private static CreateTodoCommand CreateCommand(string? title, int? order = null) =>
        new(new TodoCreateDto { Title = title, Order = order });


    [Theory]
    [MemberData(nameof(Cases))]
    public Task CreateTodoCommandValidator_snapshot(string caseName, string? title, int? order)
    {
        var sut = Shape(Validate(title, order));
        return Verify(sut);
    }

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
