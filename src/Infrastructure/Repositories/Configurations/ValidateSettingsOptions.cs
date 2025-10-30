using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.Configurations;

[OptionsValidator]
public partial class ValidateSettingsOptions : IValidateOptions<SettingsOptions>
{

}
