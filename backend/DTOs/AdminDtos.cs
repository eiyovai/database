namespace CampusVisitorApi.DTOs;

public class SaveOpenRuleRequest
{
    public int? AreaId { get; set; }
    public string DateType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string? MorningStart { get; set; }
    public string? MorningEnd { get; set; }
    public string? AfternoonStart { get; set; }
    public string? AfternoonEnd { get; set; }
    public int MaxCapacity { get; set; }
    public bool IsActive { get; set; }
    public string? Remark { get; set; }
}

public class SaveAreaRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = "public";
    public string AccessLevel { get; set; } = "public";
    public List<string> AllowedTypes { get; set; } = new();
    public string? Description { get; set; }
    public string? MorningStart { get; set; }
    public string? MorningEnd { get; set; }
    public string? AfternoonStart { get; set; }
    public string? AfternoonEnd { get; set; }
}

public class CreateScheduleRequest
{
    public string StaffName { get; set; } = string.Empty;
    public string StaffRole { get; set; } = string.Empty;
    public DateTime WorkDate { get; set; }
    public string Shift { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Task { get; set; }
}

public class ReviewReportRequest
{
    public string Status { get; set; } = string.Empty; // approved / rejected
    public string? Remark { get; set; }
}

public class AddBlacklistRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class DashboardStatsResponse
{
    public int TodayReservations { get; set; }
    public int CurrentVisitors { get; set; }
    public int PendingReviews { get; set; }
    public int BlacklistCount { get; set; }
}
