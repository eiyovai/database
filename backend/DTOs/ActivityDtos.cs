namespace CampusVisitorApi.DTOs;

public class CreateActivityRequest
{
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxParticipants { get; set; }
}

public class RegisterActivityRequest
{
    public int ActivityId { get; set; }
}

public class ActivityListResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Registered { get; set; }
    public int MaxParticipants { get; set; }
}

public class ActivityRegistrationResponse
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public string ActivityTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string VisitorName { get; set; } = string.Empty;
    public string VisitorPhone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
