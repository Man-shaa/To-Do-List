using Application.Todos;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Configurations;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddInfrastructureServices(this WebApplicationBuilder builder)
    {
        AddTodoDbContext(builder);
        
        builder.Services.AddOptions<SettingsOptions>()
            .BindConfiguration(SettingsOptions.ConfigurationSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<IValidateOptions<SettingsOptions>, ValidateSettingsOptions>();

        return builder;
    }

    private static void AddTodoDbContext(this WebApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<Persistence.TodoDbContext>(AspireConfiguration.AspireResourcesName.TodoDatabase,
            null,
            options =>
            {
                options.UseNpgsql(npgsqlOptionsAction =>
                    npgsqlOptionsAction.ConfigureDataSource(dataSourceBuilder =>
                        {
                            dataSourceBuilder.EnableDynamicJson();
                            if (builder.Environment.IsDevelopment())
                                dataSourceBuilder.ConnectionStringBuilder.IncludeErrorDetail = true;
                        }
                    ));
                if (!builder.Environment.IsDevelopment())
                    return;

                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            });

        builder.EnrichNpgsqlDbContext<Persistence.TodoDbContext>();

        builder.Services.AddScoped<ITodoRepository, TodoRepository>();
    }
}
