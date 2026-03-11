using AutoMapper;
using UserService.Application.DTOs;
using UserService.Domain.Entities;

namespace UserService.Application.Mappings;

public class UserOrganizationRoleProfile : Profile
{
    public UserOrganizationRoleProfile()
    {
        CreateMap<UserOrganizationRole, UserOrganizationRoleResponse>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
    }
}
