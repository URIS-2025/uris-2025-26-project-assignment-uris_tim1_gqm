using AutoMapper;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Application.Mappings;

public class GqmGoalMappingProfile : Profile
{
    public GqmGoalMappingProfile()
    {
        CreateMap<GqmGoalRequest, GqmGoal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<GqmGoal, GqmGoalResponse>();
    }
}
