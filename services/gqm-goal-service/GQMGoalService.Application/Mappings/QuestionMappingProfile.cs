using AutoMapper;
using GQMGoalService.Application.DTOs.Question;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Application.Mappings;

public class QuestionMappingProfile : Profile
{
    public QuestionMappingProfile()
    {
        CreateMap<QuestionRequest, Question>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        CreateMap<Question, QuestionResponse>();
    }
}
