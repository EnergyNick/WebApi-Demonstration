namespace UsersManager.Domain.Models;

public record CreateUserRequest
{
    public string Login { get; init; }
    public string Password { get; init; }
    public GroupCode Group { get; init; }
}