namespace UsersManager.Service.Models;

public record UserDto(string Login,
    DateTime CreatedDate,
    UserGroupDto Group,
    UserStateDto State);