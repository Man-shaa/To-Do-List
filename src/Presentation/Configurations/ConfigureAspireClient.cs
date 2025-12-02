using ServiceDefaults;

namespace Presentation.Configurations;

public static class ConfigureAspireClient
{
    public static WebApplicationBuilder AddAspireClientConfiguration(this WebApplicationBuilder applicationBuilder)
    {
        applicationBuilder.AddServiceDefaults();

        return applicationBuilder;
    }
}
