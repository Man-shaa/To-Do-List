using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Application.Todos.Commands.UpdateTodo;

public sealed class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
{
    private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;

    public UpdateTodoCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatchDocument)
            .NotNull()
            .WithMessage("PatchDocument is required.");

        RuleFor(x => x.PatchDocument!.Operations)
            .NotNull()
            .NotEmpty()
            .WithMessage("PatchDocument must have at least one operation.");

        RuleFor(x => x.PatchDocument!)
            .Custom((patch, context) =>
            {
                if (patch.Operations.Any(op => op.OperationType != OperationType.Replace))
                    context.AddFailure("PatchDocument", "Only 'replace' operations are permitted.");

                foreach (var op in patch.Operations)
                {
                    if (string.IsNullOrWhiteSpace(op.path))
                    {
                        context.AddFailure("PatchDocument", "Each operation must have a non-empty 'path'.");
                        continue;
                    }
                    if (!op.path.StartsWith("/", StringComparison.Ordinal))
                    {
                        context.AddFailure("PatchDocument", "Path must start with '/'.");
                        continue;
                    }

                    if (op.OperationType == OperationType.Replace)
                    {
                        if (op.value is null)
                            context.AddFailure("PatchDocument", "Value field required.");
                    }
                }

                var temp = new Todo(id: 12, "Title", new Uri("https://localhost/todos/12"), order: 3);
                var applyErrors = new List<string>();

                try
                {
                    patch.ApplyTo(temp, error => applyErrors.Add(error.ErrorMessage));
                }
                catch (Exception ex)
                {
                    context.AddFailure("PatchDocument", $"Failed to apply patch: {ex.Message}");
                }

                foreach (var err in applyErrors)
                    context.AddFailure("PatchDocument", err);
            });
    }
}
