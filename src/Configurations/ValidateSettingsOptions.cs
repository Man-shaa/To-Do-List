using Microsoft.Extensions.Options;

namespace Todo.Presentation.Configurations;

[OptionsValidator]
public partial class ValidateSettingsOptions : IValidateOptions<SettingsOptions>
{
}