using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ITodoService, TodoService>();
        builder.Services.AddOptions<SettingsOptions>()
            .BindConfiguration(SettingsOptions.ConfigurationSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.AddNpgsqlDbContext<TodoDbContext>("postgres");

        builder.Services.AddScoped<ITodoRepository, TodoRepository>();

        builder.Services.AddSingleton<IValidateOptions<SettingsOptions>, ValidateSettingsOptions>();

        return builder;
    }
}
