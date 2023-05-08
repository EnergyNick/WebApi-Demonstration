using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UsersManager.Domain.Models;
using UsersManager.Infrastructure;
using UsersManager.Infrastructure.Extensions;
using UsersManager.Infrastructure.Models;

namespace UsersManager.Domain.Services;

public class UserService : IUserService
{
    private readonly UsersContext _context;
    private readonly IMapper _mapper;
    private readonly ServicesSettings _options;

    public UserService(UsersContext context, IMapper mapper, IOptions<ServicesSettings> options)
    {
        _context = context;
        _mapper = mapper;
        _options = options.Value;
    }

    public async Task<User> CreateUser(CreateUserRequest userInfo, CancellationToken token)
    {
        var existingUser = await GetUsers().FirstOrDefaultAsync(x => x.Login == userInfo.Login, token);
        if (existingUser is not null
            && StateCode.Blocked != _mapper.Map<StateCode>(existingUser.State.Code))
        {
            throw new InvalidOperationException("Can't create user with existing login");
        }

        var groupCode = _mapper.Map<DbUserGroupCode>(userInfo.Group);
        if (userInfo.Group == GroupCode.Admin)
        {
            var admin = await GetActiveUsers().FirstOrDefaultAsync(x => x.Group.Code == DbUserGroupCode.Admin, token);
            if (admin is not null)
                throw new InvalidOperationException("Can't create another user with admin privileges");
        }

        var hashed = SHA256.HashData(Encoding.UTF8.GetBytes(userInfo.Password));
        var password = Encoding.UTF8.GetString(hashed);

        var user = new DbUser
        {
            Id = Guid.NewGuid().ToString(),
            Login = userInfo.Login,
            PasswordHash = password,
            CreatedDate = DateTime.UtcNow,
            Group = new DbUserGroup
            {
                Id = Guid.NewGuid().ToString(),
                Code = groupCode
            },
            State = new DbUserState
            {
                Id = Guid.NewGuid().ToString(),
                Code = _mapper.Map<DbUserStateCode>(StateCode.Created)
            }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Added a special 'waiting' for task requirements
        await Task.Delay(_options.UserCreationTimeout, CancellationToken.None);

        user.State.Code = _mapper.Map<DbUserStateCode>(StateCode.Active);
        await _context.SaveChangesAsync(CancellationToken.None);

        return _mapper.Map<User>(user);
    }

    public async Task<bool> DeleteUser(string login, CancellationToken token)
    {
        var user = await GetActiveUsers().FirstOrDefaultAsync(x => x.Login == login, token);

        if (user is null)
            return false;

        user.State.Code = _mapper.Map<DbUserStateCode>(StateCode.Blocked);
        await _context.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<User?> GetUser(string login, CancellationToken token)
    {
        var user = await GetActiveUsers().FirstOrDefaultAsync(x => x.Login == login, token);

        return user is not null ? _mapper.Map<User>(user) : null;
    }

    public async Task<IEnumerable<User>> GetUsers(int? pageSize, int? pageIndex, CancellationToken token)
    {
        var users = await GetActiveUsers()
            .PaginateIfNeeded(pageSize, pageIndex)
            .ToArrayAsync(token);

        return users.Select(_mapper.Map<User>);
    }

    public async Task<bool> Authenticate(string username, string password, CancellationToken token)
    {
        var user = await GetUser(username, token);
        if (user is null)
            return false;
        var hashData = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var passwordHashed = Encoding.UTF8.GetString(hashData);

        return passwordHashed == user.PasswordHash;
    }

    private IQueryable<DbUser> GetUsers() => _context.Users.Include(x => x.State).Include(x => x.Group);
    private IQueryable<DbUser> GetActiveUsers() => _context.Users
        .Include(x => x.State)
        .Include(x => x.Group)
        .Where(x => x.State.Code == DbUserStateCode.Active);
}