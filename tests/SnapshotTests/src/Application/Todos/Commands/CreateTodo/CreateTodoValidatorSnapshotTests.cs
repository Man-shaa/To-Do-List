using Application.Todos.Commands.CreateTodo;
using FluentValidation.Results;
using Infrastructure.Repositories.DTOs;

namespace SnapshotTests.Application.Todos.Commands.CreateTodo;

public sealed class CreateTodoValidatorSnapshotTests
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
        ["order_negative", "New Todo", -1],
    ];

    [Theory]
    [MemberData(nameof(Cases))]
    public Task CreateTodoCommandValidator_snapshot(string caseName, string? title, int? order)
    {
        var sut = Shape(Validate(title, order));
        return Verify(sut);
    }
}
