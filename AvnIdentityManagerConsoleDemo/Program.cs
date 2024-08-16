// Setup dependency injection
using AvnIdentityManager;
using AvnIdentityManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

var serviceProvider = new ServiceCollection()
    .AddLogging(config =>
    {
        config.AddConsole();
        config.SetMinimumLevel(LogLevel.Error); 
    })
    
    // Replace with an initial connection string
    .AddIdentityManager("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AuthServerDemo;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False") 
    .BuildServiceProvider();

var userManagementService = serviceProvider.GetRequiredService<UserManagementService>();

// Interactive or script mode
if (args.Length > 0)
{
    // Script mode
    ExecuteScript(userManagementService, args);
}
else
{
    // Interactive mode
    RunInteractiveMode(userManagementService);
}

static void ExecuteScript(UserManagementService service, string[] args)
{
    Console.WriteLine("Script mode not yet implemented");

    // Example script execution logic
    foreach (var arg in args)
    {
        //Console.WriteLine($"Executing command: {arg}");
        // Parse and execute commands from args
    }
}

static void RunInteractiveMode(UserManagementService service)
{
    Console.WriteLine("Welcome to the Identity Manager console demo.");

    while (true)
    {
        Console.WriteLine();

        Console.WriteLine("Choose an option:");
        Console.WriteLine("1. Create a new database");
        Console.WriteLine("2. Switch auth databases");
        Console.WriteLine("3. Add a user");
        Console.WriteLine("4. Add a role");
        Console.WriteLine("5. Modify user roles");
        Console.WriteLine("6. Delete a user");
        Console.WriteLine("7. Delete a role");
        Console.WriteLine("8. Show all users");
        Console.WriteLine("9. Show all roles");
        Console.WriteLine("10. Change user password");
        Console.Write("> ");
        var choice = Console.ReadLine();
        Console.WriteLine();
        switch (choice)
        {
            case "1":
                CreateDatabase(service);
                break;
            case "2":
                SwitchDatabase(service);
                break;
            case "3":
                AddUser(service);
                break;
            case "4":
                AddRole(service);
                break;
            case "5":
                ModifyUserRoles(service);
                break;
            case "6":
                DeleteUser(service);
                break;
            case "7":
                DeleteRole(service);
                break;
            case "8":
                ShowAllUsers(service);
                break;
            case "9":
                ShowAllRoles(service);
                break;
            case "10":
                ChangePassword(service);
                break;
            default:
                Console.WriteLine("Invalid choice");
                break;
        }
    }
}

static void CreateDatabase(UserManagementService service)
{
    Console.Write("Enter connection string: ");
    var connectionString = Console.ReadLine();
    service.InitializeDatabase(connectionString);
    Console.WriteLine("Database created successfully.");
    service.ChangeConnectionString(connectionString);
    Console.WriteLine("Switched to new database.");
}

static void SwitchDatabase(UserManagementService service)
{
    Console.Write("Enter new connection string: ");
    var connectionString = Console.ReadLine();
    service.ChangeConnectionString(connectionString);
    Console.WriteLine("Switched to new database.");
}

static void AddUser(UserManagementService service)
{
    Console.Write("Enter username: ");
    var username = Console.ReadLine();
    Console.Write("Enter name: ");
    var name = Console.ReadLine();
    Console.Write("Enter email: ");
    var email = Console.ReadLine();
    Console.Write("Enter password: ");
    var password = ReadPassword(); // Use the custom ReadPassword method

    var response = service.CreateUserAsync(username, name, email, password).Result;
    if (response.Success)
    {
        Console.WriteLine("User added successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to add user: {response.Messages}");
    }
}

static void AddRole(UserManagementService service)
{
    Console.Write("Enter role name: ");
    var roleName = Console.ReadLine();

    var response = service.CreateRoleAsync(roleName).Result;
    if (response.Success)
    {
        Console.WriteLine("Role added successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to add role: {response.Messages}");
    }
}

static void ChangePassword(UserManagementService service)
{
    ShowAllUsers(service);

    Console.Write("Enter username: ");
    var username = Console.ReadLine();

    var user = service.GetUsers(username).FirstOrDefault();
    if (user == null)
    {
        Console.WriteLine("User not found.");
        return;
    }

    Console.Write("Enter new password: ");
    var password1 = ReadPassword();

    Console.Write("Confirm new password: ");
    var password2 = ReadPassword();

    if (password1 != password2)
    {
        Console.WriteLine("Passwords do not match.");
        return;
    }

    var model = new ResetPasswordModel()
    {
        UserId = user.Id,
        NewPassword = password1,
        ConfirmPassword = password2
    };

    var response = service.HardResetPassword(model).Result;
    if (response.Success)
    {
        Console.WriteLine("User password updated successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to update user password: {response.Messages}");
    }
}

static void ModifyUserRoles(UserManagementService service)
{
    ShowAllUsers(service);

    Console.Write("Enter username: ");
    var username = Console.ReadLine();

    var user = service.GetUsers(username).FirstOrDefault();
    if (user == null)
    {
        Console.WriteLine("User not found.");
        return;
    }

    ShowAllRoles(service);
    Console.WriteLine("Current roles:");
    foreach (var role in user.Roles)
    {
        Console.WriteLine(role);
    }

    Console.Write("Enter roles to assign (comma-separated): ");
    var roles = Console.ReadLine().Split(',').ToList();

    var response = service.UpdateUserAsync(user, roles).Result;
    if (response.Success)
    {
        Console.WriteLine("User roles updated successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to update user roles: {response.Messages}");
    }
}

static void DeleteUser(UserManagementService service)
{
    Console.Write("Enter username: ");
    var username = Console.ReadLine();

    var user = service.GetUsers(username).FirstOrDefault();
    if (user == null)
    {
        Console.WriteLine("User not found.");
        return;
    }

    Console.Write("Do you want to delete the user role assignments? (y/n): ");
    var deleteRoles = Console.ReadLine().ToLower() == "y";
    var response = service.DeleteUserAsync(user.Id, deleteRoles).Result;
    if (response.Success)
    {
        Console.WriteLine("User deleted successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to delete user: {response.Messages}");
    }
}

static void DeleteRole(UserManagementService service)
{
    Console.Write("Enter role name: ");
    var roleName = Console.ReadLine();

    var role = service.GetRoles(roleName).FirstOrDefault();
    if (role == null)
    {
        Console.WriteLine("Role not found.");
        return;
    }

    Console.Write("Do you want to delete the user role assignments? (y/n): ");
    var deleteRoles = Console.ReadLine().ToLower() == "y";

    var response = service.DeleteRoleAsync(role.Id, deleteRoles).Result;
    if (response.Success)
    {
        Console.WriteLine("Role deleted successfully.");
    }
    else
    {
        Console.WriteLine($"Failed to delete role: {response.Messages}");
    }
}

static void ShowAllUsers(UserManagementService service)
{
    var users = service.GetUsers();
    foreach (var user in users)
    {
        Console.WriteLine($"Username: {user.UserName}, Email: {user.Email}, Roles: {string.Join(", ", user.Roles)}");
    }
}

static void ShowAllRoles(UserManagementService service)
{
    var roles = service.GetRoles();
    foreach (var role in roles)
    {
        Console.WriteLine($"Role Name: {role.Name}");
    }
}
static string ReadPassword()
{
    var password = new StringBuilder();
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(true);

        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Remove(password.Length - 1, 1);
            Console.Write("\b \b");
        }
    } while (key.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return password.ToString();
}