using FluentValidation;

namespace ToDo.Application.Todos.Commands.CreateTodo;

public sealed class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Todo.Title)
            .NotNull()
            .WithMessage("Title field required")
            .NotEmpty()
            .WithMessage("Title must not be empty");

        RuleFor(x => x.Todo.Order)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Todo.Order.HasValue)
            .WithMessage("Order must be positive when provided");
    }
}
