using DanceAcademy.Application.Interfaces;
using DanceAcademy.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace DanceAcademy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        return services;
    }
}
