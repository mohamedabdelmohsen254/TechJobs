namespace EgyptTechJobsAdmin.Models.DTOs;

public class JobResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? WorkType { get; set; }
    public string ApplyUrl { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? JobId { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsManualEntry { get; set; }
    public bool IsVisibleToUsers { get; set; }
    public string? Description { get; set; }
    public string? SalaryRange { get; set; }
    public string? Tags { get; set; }
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
