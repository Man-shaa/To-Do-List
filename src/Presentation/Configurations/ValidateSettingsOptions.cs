using Microsoft.Extensions.Options;

namespace ToDo.Presentation.Configurations;

[OptionsValidator]
public partial class ValidateSettingsOptions : IValidateOptions<SettingsOptions>
{
}