using UsersManager.Domain.Models;

namespace UsersManager.Domain.Services;

public interface IUserService
{
    Task<User> CreateUser(CreateUserRequest userInfo, CancellationToken token = default);
    Task<bool> DeleteUser(string login, CancellationToken token = default);

    Task<User?> GetUser(string login, CancellationToken token = default);
    Task<IEnumerable<User>> GetUsers(int? pageSize = null, int? pageIndex = null, CancellationToken token = default);

    public Task<bool> Authenticate(string username, string password, CancellationToken token = default);
}