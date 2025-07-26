using AutoMapper;
using Identity.Application.Features.User.Commands.Create;
using Identity.Domain.Aggregates;

namespace Identity.Application.Profiles;

public class UserMappingProfiles : Profile
{
    public UserMappingProfiles()
    {
        CreateMap<User, CreateUserDto>().ReverseMap();
    }
}