using AutoMapper;
using GQMGoalService.Application.DTOs.Measurement;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Application.Mappings;

public class MeasurementMappingProfile : Profile
{
    public MeasurementMappingProfile()
    {
        CreateMap<MeasurementRequest, Measurement>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<Measurement, MeasurementResponse>();
    }
}
