using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Serialization;
using Presentation.ExceptionHandlers;

namespace Presentation.Configurations;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddPresentationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
            {
                options.InputFormatters.RemoveType<SystemTextJsonInputFormatter>();
            })
            .AddNewtonsoftJson(opts =>
            {
                opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }); //remove newtonsoft json

        builder.Services.AddOpenApi();
        builder.Services.AddVersioning();
        builder.Services.AddDaprClient();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddProblemDetails();
        builder.Services.AddSingleton<IExceptionHandler, ValidationExceptionHandler>();
        builder.Services.AddOptionsWithValidateOnStart<DaprConfiguration>()
            .BindConfiguration(DaprConfiguration.SectionName)
            .ValidateDataAnnotations();
        return builder;
    }

    private static void AddVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }
}
