using AutoFixture;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Moq.EntityFrameworkCore;
using UsersManager.Domain;
using UsersManager.Domain.Models;
using UsersManager.Domain.Services;
using UsersManager.Infrastructure;
using UsersManager.Infrastructure.Models;

namespace UsersManager.Tests;

public class UserServiceTests
{
    private readonly Mock<IMapper> _mapperMoq;
    private readonly Mock<UsersContext> _contextMoq;
    private readonly Mock<IOptions<ServicesSettings>> _optionsMoq;

    private static readonly IFixture Fixture = new Fixture();

    public UserServiceTests()
    {
        _optionsMoq = new Mock<IOptions<ServicesSettings>>();
        _optionsMoq.Setup(x => x.Value).Returns(new ServicesSettings { UserCreationTimeout = new TimeSpan(0) });

        _contextMoq = new Mock<UsersContext>();
        _contextMoq.Setup(x => x.Users).ReturnsDbSet(Array.Empty<DbUser>());
        _contextMoq.Setup(x => x.Groups).ReturnsDbSet(Array.Empty<DbUserGroup>());
        _contextMoq.Setup(x => x.States).ReturnsDbSet(Array.Empty<DbUserState>());

        _mapperMoq = new Mock<IMapper>();
        _mapperMoq.Setup(x => x.Map<User>(It.IsAny<DbUser>())).Returns(It.IsAny<User>());
        _mapperMoq.Setup(x => x.Map<DbUserStateCode>(StateCode.Active)).Returns(DbUserStateCode.Active);
        _mapperMoq.Setup(x => x.Map<DbUserGroupCode>(GroupCode.Admin)).Returns(DbUserGroupCode.Admin);
        _mapperMoq.Setup(x => x.Map<StateCode>(DbUserStateCode.Blocked)).Returns(StateCode.Blocked);
        _mapperMoq.Setup(x => x.Map<StateCode>(DbUserStateCode.Active)).Returns(StateCode.Active);
    }

    [Fact]
    public async Task TestTryGetUnexcitingUser()
    {
        const string login = "Test";

        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        await service.Invoking(x => x.GetUser(login)).Should().NotThrowAsync();
        var result = await service.GetUser(login);
        result.Should().BeNull();

        var multiplyResult = await service.GetUsers();
        multiplyResult.Should().BeEmpty();
    }

    [Fact]
    public async Task TestGetExistingUser()
    {
        const string login = "Test";
        var user = CreateUser(login);
        var dbUser = CreateDbUser(login);

        _contextMoq.Setup(x => x.Users).ReturnsDbSet(new []{dbUser});
        _mapperMoq.Setup(x => x.Map<User>(dbUser)).Returns(user);
        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        await service.Invoking(x => x.GetUser(login)).Should().NotThrowAsync();
        var result = await service.GetUser(login);
        result.Should().Be(user);

        var multiplyResult = await service.GetUsers();
        multiplyResult.Should().Contain(user).And.ContainSingle();
    }

    [Fact]
    public async Task TestNotGetUnActiveOrDeletedUser()
    {
        const string login = "Test";
        var userDeleted = CreateUser(login, StateCode.Blocked);
        var dbUserDeleted = CreateDbUser(login, DbUserStateCode.Blocked);
        var userUnActive = CreateUser(login, StateCode.Created);
        var dbUserUnActive = CreateDbUser(login, DbUserStateCode.Created);

        _contextMoq.Setup(x => x.Users).ReturnsDbSet(new []{dbUserUnActive, dbUserDeleted});
        _mapperMoq.Setup(x => x.Map<User>(dbUserDeleted)).Returns(userDeleted);
        _mapperMoq.Setup(x => x.Map<User>(dbUserUnActive)).Returns(userUnActive);
        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        var result = await service.GetUser(login);
        result.Should().BeNull();
        var multiplyResult = await service.GetUsers();
        multiplyResult.Should().BeEmpty();
    }

    [Fact]
    public async Task TestCreateCorrectUser()
    {
        const string login = "Test";
        var request = new CreateUserRequest
        {
            Login = login,
            Group = GroupCode.User,
            Password = "12345"
        };

        var resultUser = CreateUser(login);

        _mapperMoq
            .Setup(x => x.Map<User>(It.Is<DbUser>(u => u.Login == login
                                                       && u.State.Code == DbUserStateCode.Active
                                                       && u.Group.Code == DbUserGroupCode.User)))
            .Returns(resultUser);
        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        var result = await service.CreateUser(request);
        result.Should().Be(resultUser);
    }

    [Fact]
    public async Task TestNotCreateSameUser()
    {
        const string login = "Test";
        var request = new CreateUserRequest
        {
            Login = login,
            Group = GroupCode.User,
            Password = "12345"
        };

        var otherUser = CreateDbUser(login);
        _contextMoq.Setup(x => x.Users).ReturnsDbSet(new []{otherUser});

        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        await service.Invoking(x => x.CreateUser(request)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task TestCreateCorrectAdmin()
    {
        const string login = "Test";
        var request = new CreateUserRequest
        {
            Login = login,
            Group = GroupCode.Admin,
            Password = "12345"
        };

        var resultUser = CreateUser(login, group: GroupCode.Admin);

        _mapperMoq
            .Setup(x => x.Map<User>(It.Is<DbUser>(u => u.Login == login
                                                       && u.State.Code == DbUserStateCode.Active
                                                       && u.Group.Code == DbUserGroupCode.Admin)))
            .Returns(resultUser);
        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        var result = await service.CreateUser(request);
        result.Should().Be(resultUser);
    }

    [Fact]
    public async Task TestIncorrectCreateSecondAdmin()
    {
        const string login = "Test";
        var request = new CreateUserRequest
        {
            Login = login,
            Group = GroupCode.Admin,
            Password = "12345"
        };

        var otherDbAdmin = CreateDbUser(login, group: DbUserGroupCode.Admin);
        _contextMoq.Setup(x => x.Users).ReturnsDbSet(new []{otherDbAdmin});

        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        await service.Invoking(x => x.CreateUser(request)).Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task TestCreateCorrectUserWithDeletedUserWithSameLogin()
    {
        const string login = "Test";
        var request = new CreateUserRequest
        {
            Login = login,
            Group = GroupCode.User,
            Password = "12345"
        };

        var resultUser = CreateUser(login);
        var dbUser = CreateDbUser(login, state: DbUserStateCode.Blocked);
        _contextMoq.Setup(x => x.Users).ReturnsDbSet(new []{dbUser});

        _mapperMoq
            .Setup(x => x.Map<User>(It.Is<DbUser>(u => u.Login == login
                                                       && u.State.Code == DbUserStateCode.Active
                                                       && u.Group.Code == DbUserGroupCode.User)))
            .Returns(resultUser);
        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        var result = await service.CreateUser(request);
        result.Should().Be(resultUser);
    }

    [Fact]
    public async Task TestDeleteExistingUser()
    {
        const string login = "Test";

        var dbUser = CreateDbUser(login);
        _contextMoq.Setup(x => x.Users).ReturnsDbSet(new []{dbUser});

        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        var result = await service.DeleteUser(login);
        result.Should().Be(true);
    }

    [Fact]
    public async Task TestDeleteUnexcitingOrDeletedUser()
    {
        const string login = "Test";
        const string loginDeleted = "TestDeleted";

        var dbUser = CreateDbUser(loginDeleted, state: DbUserStateCode.Blocked);
        _contextMoq.Setup(x => x.Users).ReturnsDbSet(new []{dbUser});

        var service = new UserService(_contextMoq.Object, _mapperMoq.Object, _optionsMoq.Object);

        var result = await service.DeleteUser(login);
        result.Should().Be(false);

        var result2 = await service.DeleteUser(loginDeleted);
        result2.Should().Be(false);
    }

    private static User CreateUser(string login,
        StateCode state = StateCode.Active,
        GroupCode group = GroupCode.User) =>
        new() { Login = login, State = new UserState { Code = state }, Group = new UserGroup { Code = group } };

    private static DbUser CreateDbUser(string login,
        DbUserStateCode state = DbUserStateCode.Active,
        DbUserGroupCode group = DbUserGroupCode.User) =>
        new() { Login = login, State = new DbUserState { Code = state }, Group = new DbUserGroup { Code = group } };
}