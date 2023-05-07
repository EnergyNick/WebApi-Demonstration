using AutoMapper;
using UsersManager.Infrastructure.Models;

namespace UsersManager.Domain.Models.Profiles;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
        CreateMap<User, DbUser>().ReverseMap();

        CreateMap<UserGroup, DbUserGroup>().ReverseMap();
        CreateMap<GroupCode, DbUserGroupCode>().ReverseMap();

        CreateMap<UserState, DbUserState>().ReverseMap();
        CreateMap<StateCode, DbUserStateCode>().ReverseMap();
    }
}