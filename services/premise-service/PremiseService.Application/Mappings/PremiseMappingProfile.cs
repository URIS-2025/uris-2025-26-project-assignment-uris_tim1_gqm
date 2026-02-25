using AutoMapper;
using PremiseService.Application.DTOs;
using PremiseService.Domain.Entities;

namespace PremiseService.Application.Mappings;

public class PremiseMappingProfile : Profile
{
    public PremiseMappingProfile()
    {
        CreateMap<Premise, PremiseResponse>()
            .ForMember(dest => dest.NewVersionOf, opt => opt.MapFrom(src => src.NewVersionOfId));

        CreateMap<Premise, PremiseActiveResponse>();

        CreateMap<PremiseRequest, Premise>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.NewVersionOfId, opt => opt.Ignore())
            .ForMember(dest => dest.NewVersionOf, opt => opt.Ignore())
            .ForMember(dest => dest.NewerVersion, opt => opt.Ignore());

        CreateMap<PremiseUpdateRequest, Premise>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.NewVersionOfId, opt => opt.Ignore())
            .ForMember(dest => dest.NewVersionOf, opt => opt.Ignore())
            .ForMember(dest => dest.NewerVersion, opt => opt.Ignore());
    }
}
