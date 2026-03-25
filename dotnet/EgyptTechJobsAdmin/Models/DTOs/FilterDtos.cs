namespace EgyptTechJobsAdmin.Models.DTOs;

public class BlockedCompanyDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBlockedCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class BlockedKeywordDto
{
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBlockedKeywordDto
{
    public string Keyword { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
