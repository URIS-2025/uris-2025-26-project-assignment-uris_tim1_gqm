using AutoMapper;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Application.DTOs.Question;
using GQMGoalService.Application.DTOs.Target;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GqmGoalRequest, GqmGoal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<GqmGoal, GqmGoalResponse>();

        CreateMap<QuestionRequest, Question>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<Question, QuestionResponse>();

        CreateMap<TargetRequest, Target>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<Target, TargetResponse>();

        CreateMap<MeasurementRequest, Measurement>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<Measurement, MeasurementResponse>();
    }
}
