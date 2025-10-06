using System.ComponentModel.DataAnnotations;

namespace Todo.Infrastructure.Repositories.Configurations;

public sealed class SettingsOptions
{
    public const string ConfigurationSectionName = "TodoSettings";

    [Required]
    public required Uri BaseUrl { get; init; }
}
