using AuditService.Application.DTOs;
using AuditService.Domain.Entities;
using AutoMapper;

namespace AuditService.Application.Mappings;

public class AuditLogProfile : Profile
{
    public AuditLogProfile()
    {
        CreateMap<AuditLog, AuditLogResponse>();
    }
}
