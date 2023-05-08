using UsersManager.Domain.Models;

namespace UsersManager.Service.Models;

public record CreateUserRequestDto(string Login, string Password, GroupCode Group);