using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Application.Todos.Commands.UpdateTodo;

public sealed class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PatchDocument)
            .NotNull()
            .WithMessage("PatchDocument is required.");

        RuleFor(x => x.PatchDocument.Operations)
            .NotNull()
            .NotEmpty()
            .WithMessage("PatchDocument must have at least one operation.");
        
        RuleFor(x => x.PatchDocument)
            .Custom((patch, context) =>
            {
                if (patch.Operations.Any(op => op.OperationType != OperationType.Replace))
                    context.AddFailure("PatchDocument", "Only 'replace' operations are permitted.");

                foreach (var op in patch.Operations.Where(op => string.Equals(op.path, "/title", StringComparison.OrdinalIgnoreCase)))
                {
                    if (string.IsNullOrWhiteSpace(op.value.ToString()))
                        context.AddFailure("PatchDocument", "Title field required.");
                }

                var temp = new Todo(id:12, "Title", new Uri("https://localhost/todos/12"), order:3);
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
