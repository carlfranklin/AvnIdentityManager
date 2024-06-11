using System.ComponentModel.DataAnnotations;

namespace AvnIdentityManager;

/// <summary>
/// Used to create a new user
/// </summary>
public class CreateUserModel
{
    [Required]
    public string? UserName { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
}