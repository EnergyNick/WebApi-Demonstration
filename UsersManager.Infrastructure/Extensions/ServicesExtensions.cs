using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UsersManager.Infrastructure.Extensions;

public static class ServicesExtensions
{
    public static void AddDataBaseInfrastructure(this IServiceCollection services)
    {
        services.AddDbContextPool<UsersContext>((provider, builder) =>
        {
            var options = provider.GetService<IOptions<DataBaseSettings>>();
            var cfg = options?.Value
                ?? throw new InvalidOperationException("Can't create database context without connection settings");

            builder.UseNpgsql(cfg.ConnectionString);
        });
    }
}