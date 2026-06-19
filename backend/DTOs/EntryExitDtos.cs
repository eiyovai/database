namespace CampusVisitorApi.DTOs;

public class EntryCheckRequest
{
    public string? Query { get; set; }
    public int? Id { get; set; }
    public string? Gate { get; set; }
}

public class ExitRecordRequest
{
    public string Name { get; set; } = string.Empty;
    public string Gate { get; set; } = string.Empty;
}
