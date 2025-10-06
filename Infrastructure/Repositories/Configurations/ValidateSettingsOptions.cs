using Microsoft.Extensions.Options;

namespace Todo.Infrastructure.Repositories.Configurations;

[OptionsValidator]
public partial class ValidateSettingsOptions : IValidateOptions<SettingsOptions>
{
}
