using Microsoft.Extensions.DependencyInjection;
using UsersManager.Domain.Services;

namespace UsersManager.Domain.Extensions;

public static class ServicesExtensions
{
    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddTransient<IUserService, UserService>();
    }
}