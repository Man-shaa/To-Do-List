using Infrastructure.Repositories;
using Infrastructure.Repositories.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ITodoService, TodoService>();
        services.AddOptions<SettingsOptions>()
            .BindConfiguration(SettingsOptions.ConfigurationSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<SettingsOptions>, ValidateSettingsOptions>();

        return services;
    }
}
