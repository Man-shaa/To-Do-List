namespace Infrastructure.Repositories.Configurations;

public sealed class SettingsOptions
{
    public const string ConfigurationSectionName = "TodoSettings";

    public required Uri? BaseUrl { get; init; }
}
