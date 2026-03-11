using AutoMapper;
using DepartmentService.Application.DTOs;
using DepartmentService.Domain.Entities;

namespace DepartmentService.Application.Mappings;

public class OrganizationProfile : Profile
{
    public OrganizationProfile()
    {
        CreateMap<Organization, OrganizationResponse>();
        CreateMap<OrganizationRequest, Organization>();
    }
}
