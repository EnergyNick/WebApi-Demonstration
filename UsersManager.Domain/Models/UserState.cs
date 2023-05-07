namespace UsersManager.Domain.Models;

public record UserState
{
    public string Id { get; init; }
    public StateCode Code { get; init; }
    public string Description { get; init; }
}