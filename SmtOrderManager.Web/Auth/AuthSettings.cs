namespace SmtOrderManager.Web.Auth;

public sealed class AuthSettings
{
    public List<AuthUser> Users { get; init; } = new();
}

public sealed class AuthUser
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = "User";
}
