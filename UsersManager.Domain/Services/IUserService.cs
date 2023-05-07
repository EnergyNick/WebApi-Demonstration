using UsersManager.Domain.Models;

namespace UsersManager.Domain.Services;

public interface IUserService
{
    Task<User> CreateUser(CreateUserRequest userInfo, CancellationToken token = default);
    Task<bool> DeleteUser(string login, CancellationToken token = default);

    Task<User?> GetUser(string login, CancellationToken token = default);
    Task<IEnumerable<User>> GetUsers(int? pageSize, int? pageIndex, CancellationToken token = default);
}