using CampusVisitorApi.DTOs;

namespace CampusVisitorApi.Services;

public interface IReservationService
{
    Task CreateAsync(int userId, CreateReservationRequest request);
    Task<PagedResult<ReservationListResponse>> GetMyReservationsAsync(int userId, string? status, int page, int pageSize);
    Task<ReservationListResponse> GetDetailAsync(int id);
    Task CancelAsync(int userId, int id);

    // Admin
    Task<PagedResult<ReservationListResponse>> GetListAsync(string? status, int page, int pageSize);
    Task ReviewAsync(int reviewerId, int id, ReviewReservationRequest request);
}
