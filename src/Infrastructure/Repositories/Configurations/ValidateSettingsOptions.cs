using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.Configurations;

[OptionsValidator]
public partial class ValidateSettingsOptions : IValidateOptions<SettingsOptions>
{
    public ValidateOptionsResult Validate(string? name, SettingsOptions options)
    {
        throw new NotImplementedException();
    }
}
