namespace CampusVisitorApi.DTOs;

public class CreateReportRequest
{
    public string Target { get; set; } = string.Empty;
    public string ViolationType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Description { get; set; } = string.Empty;
}
