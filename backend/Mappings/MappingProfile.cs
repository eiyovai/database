using AutoMapper;
using CampusVisitorApi.DTOs;
using CampusVisitorApi.Models;

namespace CampusVisitorApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User
        CreateMap<User, UserInfo>();

        // Reservation
        CreateMap<CreateReservationRequest, Reservation>();
        CreateMap<Reservation, ReservationListResponse>()
            .ForMember(d => d.EntryTime, o => o.MapFrom(s =>
                s.EntryExitRecords.FirstOrDefault(r => r.EntryTime != null) != null
                    ? s.EntryExitRecords.FirstOrDefault(r => r.EntryTime != null)!.EntryTime
                    : (DateTime?)null))
            .ForMember(d => d.ExitTime, o => o.MapFrom(s =>
                s.EntryExitRecords.FirstOrDefault(r => r.ExitTime != null) != null
                    ? s.EntryExitRecords.FirstOrDefault(r => r.ExitTime != null)!.ExitTime
                    : (DateTime?)null))
            .ForMember(d => d.EntryGate, o => o.MapFrom(s =>
                s.EntryExitRecords.FirstOrDefault(r => r.EntryGate != null) != null
                    ? s.EntryExitRecords.FirstOrDefault(r => r.EntryGate != null)!.EntryGate!.Name
                    : null));

        // Activity
        CreateMap<Activity, ActivityListResponse>()
            .ForMember(d => d.Registered, o => o.MapFrom(s => s.CurrentCount));

        // OpenRule
        CreateMap<SaveOpenRuleRequest, OpenRule>();

        // StaffSchedule
        CreateMap<CreateScheduleRequest, StaffSchedule>();
    }
}
