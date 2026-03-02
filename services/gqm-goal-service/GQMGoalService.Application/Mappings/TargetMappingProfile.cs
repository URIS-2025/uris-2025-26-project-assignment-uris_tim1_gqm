using AutoMapper;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Application.Mappings;

public class TargetMappingProfile : Profile
{
    public TargetMappingProfile()
    {
        CreateMap<TargetRequest, Target>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<Target, TargetResponse>();
    }
}
