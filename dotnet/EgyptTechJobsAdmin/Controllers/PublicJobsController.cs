using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgyptTechJobsAdmin.Data;
using EgyptTechJobsAdmin.Models.DTOs;

namespace EgyptTechJobsAdmin.Controllers;

/// <summary>
/// Public API for user-facing frontend (no auth required)
/// </summary>
[ApiController]
[Route("api/public")]
public class PublicJobsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PublicJobsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all visible jobs for users (public endpoint)
    /// </summary>
    [HttpGet("jobs")]
    public async Task<ActionResult<PaginatedResponse<JobResponseDto>>> GetVisibleJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? country = null,
        [FromQuery] string? workType = null,
        [FromQuery] string? source = null)
    {
        var query = _context.Jobs
            .Where(j => j.IsActive && j.IsVisibleToUsers)
            .AsQueryable();

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

        if (!string.IsNullOrEmpty(source))
        {
            query = query.Where(j => j.Source == source);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(j => j.PostedDate ?? j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobResponseDto
            {
                Id = j.Id,
                Title = j.Title,
                Company = j.Company,
                Location = j.Location,
                Country = j.Country,
                City = j.City,
                WorkType = j.WorkType,
                ApplyUrl = j.ApplyUrl,
                Source = j.Source,
                JobId = j.JobId,
                PostedDate = j.PostedDate,
                CreatedAt = j.CreatedAt,
                UpdatedAt = j.UpdatedAt,
                IsActive = j.IsActive,
                IsManualEntry = j.IsManualEntry,
                IsVisibleToUsers = j.IsVisibleToUsers,
                Description = j.Description,
                SalaryRange = j.SalaryRange,
                Tags = j.Tags
            })
            .ToListAsync();

        return Ok(new PaginatedResponse<JobResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Get job details by ID (only if visible)
    /// </summary>
    [HttpGet("jobs/{id}")]
    public async Task<ActionResult<JobResponseDto>> GetJob(int id)
    {
        var job = await _context.Jobs
            .Where(j => j.Id == id && j.IsActive && j.IsVisibleToUsers)
            .FirstOrDefaultAsync();

        if (job == null)
            return NotFound(new { message = "Job not found" });

        return Ok(new JobResponseDto
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
        });
    }

    /// <summary>
    /// Get statistics for public display
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetPublicStats()
    {
        var query = _context.Jobs.Where(j => j.IsActive && j.IsVisibleToUsers);

        return Ok(new
        {
            TotalJobs = await query.CountAsync(),
            JobsByCountry = await query
                .Where(j => j.Country != null)
                .GroupBy(j => j.Country!)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Country, x => x.Count),
            JobsByWorkType = await query
                .Where(j => j.WorkType != null)
                .GroupBy(j => j.WorkType!)
                .Select(g => new { WorkType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.WorkType, x => x.Count),
            JobsBySource = await query
                .Where(j => j.Source != null)
                .GroupBy(j => j.Source!)
                .Select(g => new { Source = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Source, x => x.Count)
        });
    }

    /// <summary>
    /// Get filter options (countries, work types, sources)
    /// </summary>
    [HttpGet("filters")]
    public async Task<ActionResult> GetFilters()
    {
        var query = _context.Jobs.Where(j => j.IsActive && j.IsVisibleToUsers);

        return Ok(new
        {
            Countries = await query
                .Where(j => j.Country != null)
                .Select(j => j.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(),
            WorkTypes = await query
                .Where(j => j.WorkType != null)
                .Select(j => j.WorkType)
                .Distinct()
                .OrderBy(w => w)
                .ToListAsync(),
            Sources = await query
                .Where(j => j.Source != null)
                .Select(j => j.Source)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync()
        });
    }
}
