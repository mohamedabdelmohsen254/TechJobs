using EgyptTechJobsAdmin.Models.DTOs;
using EgyptTechJobsAdmin.Models.Entities;

namespace EgyptTechJobsAdmin.Services;

public interface IJobService
{
    Task<PaginatedResponse<JobResponseDto>> GetJobsAsync(int page, int pageSize, string? search, string? country, string? workType, bool? isActive);
    Task<JobResponseDto?> GetJobByIdAsync(int id);
    Task<JobResponseDto> CreateJobAsync(CreateJobDto dto);
    Task<JobResponseDto?> UpdateJobAsync(int id, UpdateJobDto dto);
    Task<bool> DeleteJobAsync(int id);
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}

public class DashboardStatsDto
{
    public int TotalJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int VisibleJobs { get; set; }
    public int HiddenJobs { get; set; }
    public int ManualEntries { get; set; }
    public int JobsAddedToday { get; set; }
    public Dictionary<string, int> JobsByCountry { get; set; } = new();
    public Dictionary<string, int> JobsByWorkType { get; set; } = new();
    public Dictionary<string, int> JobsBySource { get; set; } = new();
}
