using Serilog;

namespace UsersManager.Service.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void AddLoggingProvider(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    public static void AddOptionWithValidate<T>(this IServiceCollection services, string sectionName)
        where T : class
    {
        services.AddOptions<T>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}