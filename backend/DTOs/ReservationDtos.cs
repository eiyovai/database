namespace CampusVisitorApi.DTOs;

public class CreateReservationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string VisitorType { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public int Companions { get; set; }
    public string? StayDuration { get; set; }
    public string Purpose { get; set; } = string.Empty;
}

public class ReviewReservationRequest
{
    public string Status { get; set; } = string.Empty; // approved / rejected
    public string? Remark { get; set; }
}

public class ReservationListResponse
{
    public int Id { get; set; }
    public string ReservationNo { get; set; } = string.Empty;
    public string VisitorType { get; set; } = string.Empty;
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorPhone { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public int Companions { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public string? EntryGate { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
