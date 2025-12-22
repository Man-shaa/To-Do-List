using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.Configurations;

[OptionsValidator]
public sealed partial class ValidateSettingsOptions : IValidateOptions<SettingsOptions>;
