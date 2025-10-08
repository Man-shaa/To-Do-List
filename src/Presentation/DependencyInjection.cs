using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Serialization;

namespace Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<TodoService>();

        services.AddControllers(options =>
            {
                options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
            })
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

        return services;
    }
}
