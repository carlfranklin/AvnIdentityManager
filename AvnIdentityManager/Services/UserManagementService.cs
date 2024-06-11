namespace AvnIdentityManager.Services;

using AvnIdentityManager;
using AvnIdentityManager.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

public class UserManagementService
{
    private UserManager<IMApplicationUser> _userManager;
    private RoleManager<IMApplicationRole> _roleManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory _dbContextFactory;

    public Dictionary<string, string> Roles;
    public Dictionary<string, string> ClaimTypes;

    public UserManagementService(UserManager<IMApplicationUser> userManager,
        RoleManager<IMApplicationRole> roleManager,
        IServiceProvider serviceProvider,
        IDbContextFactory dbContextFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _serviceProvider = serviceProvider;
        _dbContextFactory = dbContextFactory;

        Initialize();
    }

    void Initialize()
    {
        //// Set all the roles in the database, ordered by Name ascending.
        Roles = _roleManager.Roles.OrderBy(r => r.Name).ToDictionary(r => r.Id, r => r.Name);

        var fieldInfo = typeof(ClaimTypes).GetFields(BindingFlags.Static | BindingFlags.Public);

        // Set all the claim types as defined in the System.Security.Claims constants.
        ClaimTypes = fieldInfo.ToDictionary(i => i.Name, i => (string)i.GetValue(null));
    }

    /// <summary>
    /// Create a user in the database.
    /// </summary>
    /// <param name="userName">Username for the account.</param>
    /// <param name="name">Name of the user.</param>
    /// <param name="email">Email of the user.</param>
    /// <param name="password">Password for the user.</param>
    /// <returns>Response object.</returns>
    /// <exception cref="ArgumentNullException">When any of the arguments are not provided, an ArgumentNullException will be thrown.</exception>
    public async Task<Response> CreateUser(string userName, string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentNullException("userName", "The argument userName cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException("name", "The argument name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException("email", "The argument email cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException("password", "The argument password cannot be null or empty.");

        var response = new Response();
        var user = new IMApplicationUser() { Email = email, UserName = userName };

        // Create user.
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            if (name != null)
                await _userManager.AddClaimAsync(user, new Claim(System.Security.Claims.ClaimTypes.Name, name));
        }
        else
        {
            response.Messages = result.Errors.GetAllMessages();
        }

        response.Success = result.Succeeded;

        return response;
    }



    /// <summary>
    /// Returns a collection of users from the database.
    /// </summary>
    /// <param name="filter">When provided, filter the users based on partial matches of email, and username.</param>
    /// <returns>A collection of User objects.</returns>
    public IEnumerable<IMUser> GetUsers(string? filter = null)
    {
        filter = filter?.Trim();

        // Get all users, including roles, and claims, from the database.
        var users = _userManager.Users.Include(u => u.Roles).Include(u => u.Claims);

        // Filter the user list, and order by username ascending.
        var query = users.Where(u =>
            (string.IsNullOrWhiteSpace(filter) || u.Email.Contains(filter)) ||
            (string.IsNullOrWhiteSpace(filter) || u.UserName.Contains(filter))
        ).OrderBy(u => u.UserName);

        // Execute the query and set properties.
        var res = query.ToArray();

        var result = res.Select(u => new IMUser
        {
            Id = u.Id,
            Email = u.Email,
            LockedOut = u.LockoutEnd == null ? string.Empty : "Yes",
            Roles = u.Roles.Select(r => Roles[r.RoleId]),
            //Key/Value props not camel cased (https://github.com/dotnet/corefx/issues/41309)
            Claims = u.Claims.Select(c => new KeyValuePair<string, string>(ClaimTypes.Single(x => x.Value == c.ClaimType).Key, c.ClaimValue)),
            DisplayName = u.Claims?.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.Name)?.ClaimValue,
            UserName = u.UserName
        });

        return result;
    }


    /// <summary>
    /// Get user by ID.
    /// </summary>
    /// <param name="id">ID of the user.</param>
    /// <returns>Returns the ApplicationUser object.</returns>
    /// <exception cref="ArgumentNullException">When any of the arguments are not provided, an ArgumentNullException will be thrown.</exception>
    /// <exception cref="Exception">Throws an exception when the user is not found.</exception>
    public async Task<IMApplicationUser> GetUser(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException("id", "The argument id cannot be null or empty.");

        // Gets the user.
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            throw new Exception("User not found.");

        return user;
    }

    ///// <summary>
    ///// Update the user.
    ///// </summary>
    ///// <param name="id">ID of the user.</param>
    ///// <param name="email">Email of the user.</param>
    ///// <param name="locked">Weather or not the user account is locked.</param>
    ///// <param name="roles">List of roles the user should be added to.</param>
    ///// <param name="claims">List of claims the user should be added to.</param>
    ///// <returns>Response object.</returns>
    ///// <exception cref="ArgumentNullException">When any of the arguments is not provided, an ArgumentNullException will be thrown.</exception>
    //public async Task<Response> UpdateUser(string id, string email, bool locked, string[] roles, List<KeyValuePair<string, string>> claims)
    //{
    //    if (string.IsNullOrWhiteSpace(id))
    //        throw new ArgumentNullException("id", "The argument id cannot be null or empty.");

    //    if (string.IsNullOrWhiteSpace(email))
    //        throw new ArgumentNullException("email", "The argument email cannot be null or empty.");

    //    if (roles == null)
    //        throw new ArgumentNullException("roles", "The argument roles cannot be null.");

    //    var response = new Response();

    //    try
    //    {
    //        // Gets the user by ID.
    //        var user = await _userManager.FindByIdAsync(id);
    //        if (user == null)
    //            response.Messages = "User not found.";

    //        // Update only the updatable properties.
    //        user!.Email = email;
    //        user.LockoutEnd = locked ? DateTimeOffset.MaxValue : default(DateTimeOffset?);

    //        // Update user.
    //        var result = await _userManager.UpdateAsync(user);

    //        if (result.Succeeded)
    //        {
    //            response.Messages += $"Updated user {user.UserName}";

    //            // Get the current user roles.
    //            var userRoles = await _userManager.GetRolesAsync(user);

    //            // Add specified user roles.
    //            foreach (string role in roles.Except(userRoles))
    //                await _userManager.AddToRoleAsync(user, role);

    //            // Remove any roles, not specified, from the user. 
    //            foreach (string role in userRoles.Except(roles))
    //                await _userManager.RemoveFromRoleAsync(user, role);

    //            // Get the current user claims.
    //            var userClaims = await _userManager.GetClaimsAsync(user);

    //            // Add specified user claims.
    //            foreach (var kvp in claims.Where(a => !userClaims.Any(b => ClaimTypes[a.Key] == b.Type && a.Value == b.Value)))
    //                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes[kvp.Key], kvp.Value));

    //            // Remove any claims, not specified, from the user. 
    //            foreach (var claim in userClaims.Where(a => !claims.Any(b => a.Type == ClaimTypes[b.Key] && a.Value == b.Value)))
    //                await _userManager.RemoveClaimAsync(user, claim);
    //        }
    //        else
    //            response.Messages = result.Errors.GetAllMessages();

    //        response.Success = result.Succeeded;
    //    }
    //    catch (Exception ex)
    //    {
    //        response.Messages = $"Failure updating user {id}: {ex.Message}";
    //    }

    //    return response;
    //}


    public async Task<Response> UpdateUser(IMUser iMUser, List<string> roles)
    {
        if (iMUser == null)
            throw new ArgumentNullException("iMUser", "The argument iMUser cannot be null.");

        var response = new Response();

        try
        {
            // Gets the user by ID.
            var user = await _userManager.FindByIdAsync(iMUser.Id);
            if (user == null)
                response.Messages = "User not found.";

            // Update only the updatable properties.
            user!.Email = iMUser.Email;

            // Update user.
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                response.Messages += $"Updated user {user.UserName}";

                // Get the current user roles.
                var userRoles = (await _userManager.GetRolesAsync(user)).ToList();

                var claims = iMUser.Claims.ToList();

                // Add specified user roles.
                foreach (string role in roles.Except(userRoles))
                    await _userManager.AddToRoleAsync(user, role);

                // Remove any roles, not specified, from the user. 
                foreach (string role in userRoles.Except(roles))
                    await _userManager.RemoveFromRoleAsync(user, role);

                // Get the current user claims.
                var userClaims = await _userManager.GetClaimsAsync(user);

                // Add specified user claims.
                foreach (var kvp in claims.Where(a => !userClaims.Any(b => ClaimTypes[a.Key] == b.Type && a.Value == b.Value)))
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes[kvp.Key], kvp.Value));

                // Remove any claims, not specified, from the user. 
                foreach (var claim in userClaims.Where(a => !claims.Any(b => a.Type == ClaimTypes[b.Key] && a.Value == b.Value)))
                    await _userManager.RemoveClaimAsync(user, claim);
            }
            else
                response.Messages = result.Errors.GetAllMessages();

            response.Success = result.Succeeded;
        }
        catch (Exception ex)
        {
            response.Messages = $"Failure updating user {iMUser.Id}: {ex.Message}";
        }

        return response;
    }


    /// <summary>
    /// Delete user by ID.
    /// </summary>
    /// <param name="id">ID of the user.</param>
    /// <returns>Response object.</returns>
    /// <exception cref="ArgumentNullException">When any of the arguments are not provided, an ArgumentNullException will be thrown.</exception>
    public async Task<Response> DeleteUser(string id, bool deleteUserRoleAssignments = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException("id", "The argument id cannot be null or empty.");

        var response = new Response();

        try
        {
            // Get the user.
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                response.Messages = "User not found.";

            // Delete the user.
            var result = await _userManager.DeleteAsync(user!);

            if (result.Succeeded)
                response.Messages = $"Deleted user {user!.UserName}.";
            else
                response.Messages = result.Errors.GetAllMessages();

            response.Success = result.Succeeded;

            if (deleteUserRoleAssignments)
            {
                // Get all roles for the user.
                var roles = await _userManager.GetRolesAsync(user);

                // Remove the user from all roles.
                foreach (var role in roles)
                    await _userManager.RemoveFromRoleAsync(user, role);
            }
        }
        catch (Exception ex)
        {
            response.Messages = $"Failure deleting user {id}: {ex.Message}";
        }

        return response;
    }

    /// <summary>
    /// Reset user password.
    /// </summary>
    /// <param name="id">ID of the user.</param>
    /// <param name="password">Password for the user.</param>
    /// <param name="verify">Password for verification purposes.</param>
    /// <returns>Response object.</returns>
    /// <exception cref="ArgumentNullException">When any of the arguments are not provided, an ArgumentNullException will be thrown.</exception>
    public async Task<Response> ResetPassword(string id, string password, string verify)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException("id", "The argument id cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException("password", "The argument password cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(verify))
            throw new ArgumentNullException("verify", "The argument verify cannot be null or empty.");

        var response = new Response();

        try
        {
            if (password != verify)
                response.Messages = "Passwords entered do not match.";

            // Get the user.
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                response.Messages = "User not found.";

            // Delete existing password if it exists.
            if (await _userManager.HasPasswordAsync(user!))
                await _userManager.RemovePasswordAsync(user!);

            // Add new password for the user.
            var result = await _userManager.AddPasswordAsync(user!, password);

            if (result.Succeeded)
            {
                response.Messages = $"Password reset for {user!.UserName}.";
            }
            else
                response.Messages = result.Errors.GetAllMessages();
        }
        catch (Exception ex)
        {
            response.Messages = $"Failed password reset for user {id}: {ex.Message}";
        }

        return response;
    }

    /// <summary>
    /// Get user roles.
    /// </summary>
    /// <param name="filter">When provided, filter the roles based on partial matches of role name.</param>
    /// <returns>A collection of role objects.</returns>
    public IEnumerable<IMRole> GetRoles(string? filter = null)
    {
        // Get all roles, including claims, from the database.
        var roles = _roleManager.Roles.Include(r => r.Claims);

        // Filter role list, and order by name ascending.
        var query = roles.Where(r =>
            (string.IsNullOrWhiteSpace(filter) || r.Name.Contains(filter))
        ).OrderBy(r => r.Name); ;

        // Execute the query and set properties.
        var result = query.ToArray().Select(r => new IMRole
        {
            Id = r.Id,
            Name = r.Name,
            //Key/Value props not camel cased (https://github.com/dotnet/corefx/issues/41309)
            Claims = r.Claims.Select(c => new KeyValuePair<string, string>(ClaimTypes.Single(x => x.Value == c.ClaimType).Key, c.ClaimValue))
        });

        return result;
    }

    /// <summary>
    /// Create role.
    /// </summary>
    /// <param name="name">Role name.</param>
    /// <returns>Response object.</returns>
    /// <exception cref="ArgumentNullException">When any of the arguments are not provided, an ArgumentNullException will be thrown.</exception>
    public async Task<Response> CreateRole(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException("name", "The argument name cannot be null or empty.");

        var response = new Response();
        var role = new IMApplicationRole(name);

        // Create role.
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            response.Messages = result.Errors.GetAllMessages();
        }

        response.Success = result.Succeeded;

        // Update the current collection of roles in the database.
        Roles = _roleManager.Roles.OrderBy(r => r.Name).ToDictionary(r => r.Id, r => r.Name);

        return response;
    }

    /// <summary>
    /// Update role.
    /// </summary>
    /// <param name="id">ID of the role.</param>
    /// <param name="name">Name of the role.</param>
    /// <param name="claims">List of claims the role should be added to.</param>
    /// <returns>Response object.</returns>
    /// <exception cref="ArgumentNullException">When any of the arguments are not provided, an ArgumentNullException will be thrown.</exception>
    public async Task<Response> UpdateRole(string id, string name, List<KeyValuePair<string, string>> claims)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException("id", "The argument id cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException("name", "The argument name cannot be null or empty.");

        var response = new Response();

        try
        {
            // Get role.
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                response.Messages = "Role not found.";

            // Update updatable properties.
            role!.Name = name;

            // Update role.
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                response.Messages += $"Updated role {role.Name}";

                // Get the current role claims.
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                // Add specified role claims.
                foreach (var kvp in claims.Where(a => !roleClaims.Any(b => ClaimTypes[a.Key] == b.Type && a.Value == b.Value)))
                    await _roleManager.AddClaimAsync(role, new Claim(ClaimTypes[kvp.Key], kvp.Value));

                // Remove any claims, not specified, from the role.
                foreach (var claim in roleClaims.Where(a => !claims.Any(b => a.Type == ClaimTypes[b.Key] && a.Value == b.Value)))
                    await _roleManager.RemoveClaimAsync(role, claim);
            }
            else
                response.Messages = result.Errors.GetAllMessages();

            response.Success = result.Succeeded;
        }
        catch (Exception ex)
        {
            response.Messages = $"Failure updating role {id}: {ex.Message}";
        }

        // Update the current collection of roles in the database.
        Roles = _roleManager.Roles.OrderBy(r => r.Name).ToDictionary(r => r.Id, r => r.Name);

        return response;
    }

    /// <summary>
    /// Delete role.
    /// </summary>
    /// <param name="id">ID of the role.</param>
    /// <returns>Response object.</returns>
    /// <exception cref="ArgumentNullException">When any of the arguments are not provided, an ArgumentNullException will be thrown.</exception>
    public async Task<Response> DeleteRole(string id, bool deleteUserAssignments = false)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException("id", "The argument id cannot be null or empty.");

        var response = new Response();

        try
        {
            // Get role.
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
                response.Messages = "Role not found.";

            // Delete role.
            var result = await _roleManager.DeleteAsync(role!);

            if (result.Succeeded)
                response.Messages = $"Deleted role {role!.Name}.";
            else
                response.Messages = result.Errors.GetAllMessages();

            response.Success = result.Succeeded;

            if (deleteUserAssignments)
            {
                // Get all users in the role.
                var users = await _userManager.GetUsersInRoleAsync(role.Name);

                // Remove the role from all users.
                foreach (var user in users)
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
            }
        }
        catch (Exception ex)
        {
            response.Messages = $"Failure deleting role {id}: {ex.Message}";
        }

        // Update the current collection of roles in the database.
        Roles = _roleManager.Roles.OrderBy(r => r.Name).ToDictionary(r => r.Id, r => r.Name);

        return response;
    }

    private ApplicationDbContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private UserManager<IMApplicationUser> CreateUserManager(ApplicationDbContext context)
    {
        var userStore = new UserStore<IMApplicationUser, IMApplicationRole, ApplicationDbContext>(context);

        return new UserManager<IMApplicationUser>(
            userStore,
            _serviceProvider.GetRequiredService<IOptions<IdentityOptions>>(),
            _serviceProvider.GetRequiredService<IPasswordHasher<IMApplicationUser>>(),
            _serviceProvider.GetRequiredService<IEnumerable<IUserValidator<IMApplicationUser>>>(),
            _serviceProvider.GetRequiredService<IEnumerable<IPasswordValidator<IMApplicationUser>>>(),
            _serviceProvider.GetRequiredService<ILookupNormalizer>(),
            _serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            _serviceProvider,
            _serviceProvider.GetRequiredService<ILogger<UserManager<IMApplicationUser>>>()
        );
    }

    private RoleManager<IMApplicationRole> CreateRoleManager(ApplicationDbContext context)
    {
        var roleStore = new RoleStore<IMApplicationRole, ApplicationDbContext>(context);

        return new RoleManager<IMApplicationRole>(
            roleStore,
            _serviceProvider.GetRequiredService<IEnumerable<IRoleValidator<IMApplicationRole>>>(),
            _serviceProvider.GetRequiredService<ILookupNormalizer>(),
            _serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            _serviceProvider.GetRequiredService<ILogger<RoleManager<IMApplicationRole>>>()
        );
    }

    public void ChangeConnectionString(string newConnectionString)
    {
        // pre-process the connection string
        newConnectionString = newConnectionString.Trim();
        newConnectionString = newConnectionString.Replace("\\\\", "\\");

        var newContext = CreateDbContext(newConnectionString);
        _userManager = CreateUserManager(newContext);
        _roleManager = CreateRoleManager(newContext);
        Initialize();
    }

    public void InitializeDatabase(string connectionString)
    {
        // pre-process the connection string
        connectionString = connectionString.Trim();
        connectionString = connectionString.Replace("\\\\", "\\");

        using (var context = _dbContextFactory.CreateDbContext(connectionString))
        {
            try
            {
                // Ensure the database is created.
                context.Database.EnsureCreated();

                // Apply any pending migrations.
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // we expect this. The database is still created
            }
        }
    }
}
