namespace UsersManager.Domain.Models;

public record UserGroup
{
    public string Id { get; init; }
    public GroupCode Code { get; init; }
    public string Description { get; init; }
}