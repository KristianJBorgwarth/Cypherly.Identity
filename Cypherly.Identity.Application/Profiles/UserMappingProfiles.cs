using AutoMapper;
using Cypherly.Identity.Application.Features.User.Commands.Create;
using Cypherly.Identity.Domain.Aggregates;

namespace Cypherly.Identity.Application.Profiles;

public class UserMappingProfiles : Profile
{
    public UserMappingProfiles()
    {
        CreateMap<User, CreateUserDto>().ReverseMap();
    }
}