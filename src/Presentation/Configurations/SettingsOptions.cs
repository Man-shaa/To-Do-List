using System.ComponentModel.DataAnnotations;

namespace ToDo.Presentation.Configurations;

public sealed class SettingsOptions
{
    public const string ConfigurationSectionName = "TodoSettings";

    [Required]
    public required Uri BaseUrl { get; set; }
}
