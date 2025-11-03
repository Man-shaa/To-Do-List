using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        AddCatalogDbContext(builder);

        
        builder.Services.AddOptions<SettingsOptions>()
            .BindConfiguration(SettingsOptions.ConfigurationSectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddScoped<ITodoService, TodoService>();
        builder.Services.AddSingleton<IValidateOptions<SettingsOptions>, ValidateSettingsOptions>();

        return builder;
    }

    private static void AddCatalogDbContext(this IHostApplicationBuilder builder)
    {
        // var config = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile("appsettings.json", optional: true)
        //     .AddJsonFile("appsettings.Development.json", optional: true)
        //     .AddEnvironmentVariables()
        //     .Build();
        //
        // var cs = config.GetConnectionString("postgres");
        // if (string.IsNullOrWhiteSpace(cs))
        //     throw new InvalidOperationException("Connection string 'postgres' not found (ConnectionStrings:postgres).");



        builder.AddNpgsqlDbContext<TodoDbContext>("todo-db",
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
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

        builder.EnrichNpgsqlDbContext<TodoDbContext>();

        builder.Services.AddScoped<ITodoRepository, TodoRepository>();
    }

}
