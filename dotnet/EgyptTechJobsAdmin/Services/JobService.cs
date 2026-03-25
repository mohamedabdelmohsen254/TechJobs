using Microsoft.EntityFrameworkCore;
using EgyptTechJobsAdmin.Data;
using EgyptTechJobsAdmin.Models.DTOs;
using EgyptTechJobsAdmin.Models.Entities;

namespace EgyptTechJobsAdmin.Services;

public class JobService : IJobService
{
    private readonly ApplicationDbContext _context;

    public JobService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<JobResponseDto>> GetJobsAsync(
        int page, int pageSize, string? search, string? country, string? workType, bool? isActive)
    {
        var query = _context.Jobs.AsQueryable();

        // Filters
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(j => 
                j.Title.ToLower().Contains(search.ToLower()) ||
                j.Company.ToLower().Contains(search.ToLower()));
        }

        if (!string.IsNullOrEmpty(country))
        {
            query = query.Where(j => j.Country == country);
        }

        if (!string.IsNullOrEmpty(workType))
        {
            query = query.Where(j => j.WorkType == workType);
        }

        if (isActive.HasValue)
        {
            query = query.Where(j => j.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => MapToDto(j))
            .ToListAsync();

        return new PaginatedResponse<JobResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<JobResponseDto?> GetJobByIdAsync(int id)
    {
        var job = await _context.Jobs.FindAsync(id);
        return job == null ? null : MapToDto(job);
    }

    public async Task<JobResponseDto> CreateJobAsync(CreateJobDto dto)
    {
        var job = new Job
        {
            Title = dto.Title,
            Company = dto.Company,
            Location = dto.Location,
            Country = dto.Country,
            City = dto.City,
            WorkType = dto.WorkType,
            ApplyUrl = dto.ApplyUrl,
            Source = dto.Source ?? "Manual",
            PostedDate = dto.PostedDate,
            Description = dto.Description,
            SalaryRange = dto.SalaryRange,
            Tags = dto.Tags,
            IsManualEntry = true,
            IsVisibleToUsers = dto.IsVisibleToUsers,
            JobId = GenerateJobId(dto.Title, dto.Company, dto.ApplyUrl),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        // Audit log
        await LogAuditAsync("Create", "Job", job.Id, $"Created job: {job.Title} at {job.Company}");

        return MapToDto(job);
    }

    public async Task<JobResponseDto?> UpdateJobAsync(int id, UpdateJobDto dto)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job == null) return null;

        // Update only provided fields
        if (dto.Title != null) job.Title = dto.Title;
        if (dto.Company != null) job.Company = dto.Company;
        if (dto.Location != null) job.Location = dto.Location;
        if (dto.Country != null) job.Country = dto.Country;
        if (dto.City != null) job.City = dto.City;
        if (dto.WorkType != null) job.WorkType = dto.WorkType;
        if (dto.ApplyUrl != null) job.ApplyUrl = dto.ApplyUrl;
        if (dto.Source != null) job.Source = dto.Source;
        if (dto.PostedDate.HasValue) job.PostedDate = dto.PostedDate;
        if (dto.IsActive.HasValue) job.IsActive = dto.IsActive.Value;
        if (dto.IsVisibleToUsers.HasValue) job.IsVisibleToUsers = dto.IsVisibleToUsers.Value;
        if (dto.Description != null) job.Description = dto.Description;
        if (dto.SalaryRange != null) job.SalaryRange = dto.SalaryRange;
        if (dto.Tags != null) job.Tags = dto.Tags;

        job.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Audit log
        await LogAuditAsync("Update", "Job", job.Id, $"Updated job: {job.Title}");

        return MapToDto(job);
    }

    public async Task<bool> DeleteJobAsync(int id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job == null) return false;

        var jobTitle = job.Title;
        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        // Audit log
        await LogAuditAsync("Delete", "Job", id, $"Deleted job: {jobTitle}");

        return true;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.UtcNow.Date;

        // Get all jobs for in-memory unique counting
        var allJobs = await _context.Jobs
            .Select(j => new { j.Id, j.JobId, j.IsActive, j.IsVisibleToUsers, j.IsManualEntry, j.Country, j.WorkType, j.Source, j.CreatedAt })
            .ToListAsync();

        // Use JobId for uniqueness, fallback to Id for jobs without JobId
        var uniqueJobs = allJobs
            .GroupBy(j => j.JobId ?? j.Id.ToString())
            .Select(g => g.First())
            .ToList();

        var stats = new DashboardStatsDto
        {
            TotalJobs = uniqueJobs.Count,
            ActiveJobs = uniqueJobs.Count(j => j.IsActive),
            VisibleJobs = uniqueJobs.Count(j => j.IsVisibleToUsers),
            HiddenJobs = uniqueJobs.Count(j => !j.IsVisibleToUsers),
            ManualEntries = uniqueJobs.Count(j => j.IsManualEntry),
            JobsAddedToday = uniqueJobs.Count(j => j.CreatedAt.Date == today),
            JobsByCountry = uniqueJobs
                .Where(j => j.Country != null)
                .GroupBy(j => j.Country!)
                .ToDictionary(g => g.Key, g => g.Count()),
            JobsByWorkType = uniqueJobs
                .Where(j => j.WorkType != null)
                .GroupBy(j => j.WorkType!)
                .ToDictionary(g => g.Key, g => g.Count()),
            JobsBySource = uniqueJobs
                .Where(j => j.Source != null)
                .GroupBy(j => j.Source!)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    private static JobResponseDto MapToDto(Job job)
    {
        return new JobResponseDto
        {
            Id = job.Id,
            Title = job.Title,
            Company = job.Company,
            Location = job.Location,
            Country = job.Country,
            City = job.City,
            WorkType = job.WorkType,
            ApplyUrl = job.ApplyUrl,
            Source = job.Source,
            JobId = job.JobId,
            PostedDate = job.PostedDate,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            IsActive = job.IsActive,
            IsManualEntry = job.IsManualEntry,
            IsVisibleToUsers = job.IsVisibleToUsers,
            Description = job.Description,
            SalaryRange = job.SalaryRange,
            Tags = job.Tags
        };
    }

    private string GenerateJobId(string title, string company, string url)
    {
        var input = $"{title}_{company}_{url}";
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16].ToLower();
    }

    private async Task LogAuditAsync(string action, string entityType, int entityId, string details)
    {
        var log = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            PerformedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
