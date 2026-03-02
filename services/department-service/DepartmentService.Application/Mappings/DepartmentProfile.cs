using AutoMapper;
using DepartmentService.Application.DTOs;
using DepartmentService.Domain.Entities;

namespace DepartmentService.Application.Mappings;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        CreateMap<Department, DepartmentResponse>();
        CreateMap<DepartmentRequest, Department>();
    }
}
