namespace AvnIdentityManager;

/// <summary>
/// Custom Role class that includes claims
/// </summary>
public class IMRole
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public IEnumerable<KeyValuePair<string, string>>? Claims { get; set; }
}