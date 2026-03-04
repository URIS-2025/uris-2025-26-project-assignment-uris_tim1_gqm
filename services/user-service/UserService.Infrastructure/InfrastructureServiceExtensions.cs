using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("Database connection string not configured.");

        services.AddDbContext<UserServiceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<UserServiceDbContext>());

        services.AddScoped<IUserService, UserAppService>();
        services.AddScoped<IRoleService, RoleAppService>();
        services.AddScoped<IPermissionService, PermissionAppService>();
        services.AddScoped<IUserOrganizationRoleService, UserOrganizationRoleAppService>();
        services.AddScoped<IAuthService, AuthAppService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
