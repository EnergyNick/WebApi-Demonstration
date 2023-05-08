using AutoMapper;
using UsersManager.Domain.Models;

namespace UsersManager.Service.Models.Profiles;

public class UsersDtoProfile : Profile
{
    public UsersDtoProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<UserGroup, UserGroupDto>().ReverseMap();
        CreateMap<UserState, UserStateDto>().ReverseMap();
        CreateMap<CreateUserRequest, CreateUserRequestDto>().ReverseMap();
    }
}