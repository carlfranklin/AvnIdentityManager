using AvnIdentityManager.Data;
using AvnIdentityManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace AvnIdentityManager;

public static class Extensions
{
    /// <summary>
    /// Extension method that takes a collection of IEnumerable<IdentityError> and 
    /// concatenates all error descriptions into a string.
    /// </summary>
    /// <param name="errors">Collection of IdentityError objects.</param>
    /// <returns>A string containing all error messages in the collection.</returns>
    public static string GetAllMessages(this IEnumerable<IdentityError> errors)
    {
        var result = string.Empty;

        if (errors == null)
            return result;

        foreach (var error in errors)
        {
            result += string.IsNullOrEmpty(result) ? string.Empty : " ";
            result += error.Description;
        }

        return result;
    }

    /// <summary>
    /// This is an extension method that adds IdentityManager services to the service collection.
    /// This is required to be called in the Program.cs of the host application.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString">The initial connection string, which you can change on the fly</param>
    /// <returns></returns>
    public static IServiceCollection AddIdentityManager(this IServiceCollection services, string connectionString)
    {
        // Register the connection string provider
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register custom DbContext factory
        services.AddSingleton<IDbContextFactory, DbContextFactory>();

        // Manually configure Identity services
        services.AddScoped<IUserStore<IMApplicationUser>, UserStore<IMApplicationUser, IMApplicationRole, ApplicationDbContext>>();
        services.AddScoped<IRoleStore<IMApplicationRole>, RoleStore<IMApplicationRole, ApplicationDbContext>>();
        services.AddScoped<UserManager<IMApplicationUser>>();
        services.AddScoped<RoleManager<IMApplicationRole>>();
        services.AddScoped<IUserClaimsPrincipalFactory<IMApplicationUser>, UserClaimsPrincipalFactory<IMApplicationUser, IMApplicationRole>>();

        // Manually add missing services
        services.AddScoped<IPasswordHasher<IMApplicationUser>, PasswordHasher<IMApplicationUser>>();
        services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        services.AddScoped<IdentityErrorDescriber>();
        //services.AddScoped<ISecurityStampValidator, SecurityStampValidator<IMApplicationUser>>();
        services.AddScoped<IUserClaimsPrincipalFactory<IMApplicationUser>, UserClaimsPrincipalFactory<IMApplicationUser, IMApplicationRole>>();
        services.AddScoped<IRoleValidator<IMApplicationRole>, RoleValidator<IMApplicationRole>>();
        services.AddScoped<IPasswordValidator<IMApplicationUser>, PasswordValidator<IMApplicationUser>>();
        services.AddScoped<IUserValidator<IMApplicationUser>, UserValidator<IMApplicationUser>>();

        // Add your user management service
        services.AddTransient<UserManagementService>();

        return services;
    }
}