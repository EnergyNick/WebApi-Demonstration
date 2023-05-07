namespace UsersManager.Domain.Models;

public record User
{
    public string Id { get; init; }
    public string Login { get; init; }
    public string PasswordHash { get; init; }
    public DateTime CreatedDate { get; init; }
    public UserGroup Group { get; init; }
    public UserState State { get; init; }
}