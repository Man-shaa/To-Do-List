using System.ComponentModel.DataAnnotations;

namespace ToDo.Presentation.Configurations;

public sealed class SettingsOptions
{
    public const string ConfigurationSectionName = "TodoSettings";

    [Required]
    [Url]
    public required string BaseUrl { get; set; }
}