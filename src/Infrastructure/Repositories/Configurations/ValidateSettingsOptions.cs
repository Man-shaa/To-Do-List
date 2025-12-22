using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.Configurations;

public sealed class ValidateSettingsOptions : IValidateOptions<SettingsOptions>
{
    public ValidateOptionsResult Validate(string? name, SettingsOptions? options)
    {
        if (options is null) { return ValidateOptionsResult.Fail("Settings options instance is null."); }

        if (options.BaseUrl is null)
        {
            return ValidateOptionsResult.Fail("'BaseUrl' is missing in JSON settings (section 'TodoSettings').");
        }

        return ValidateOptionsResult.Success;
    }
}
