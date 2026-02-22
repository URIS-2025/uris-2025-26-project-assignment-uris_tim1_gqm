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
        CreateMap<GqmGoalRequest, GqmGoal>();
        CreateMap<GqmGoal, GqmGoalResponse>();

        CreateMap<QuestionRequest, Question>();
        CreateMap<Question, QuestionResponse>();

        CreateMap<TargetRequest, Target>();
        CreateMap<Target, TargetResponse>();

        CreateMap<MeasurementRequest, Measurement>();
        CreateMap<Measurement, MeasurementResponse>();
    }
}
