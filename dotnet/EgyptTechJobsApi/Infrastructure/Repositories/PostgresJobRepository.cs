using Microsoft.EntityFrameworkCore;
using EgyptTechJobsApi.Application.Abstractions;
using EgyptTechJobsApi.Data;
using EgyptTechJobsApi.Models;

namespace EgyptTechJobsApi.Infrastructure.Repositories;

public class PostgresJobRepository : IJobRepository
{
    private readonly JobsDbContext _context;
    private readonly ILogger<PostgresJobRepository> _logger;

    public PostgresJobRepository(JobsDbContext context, ILogger<PostgresJobRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JobListing>> GetAllAsync()
    {
        try
        {
            var jobs = await _context.Jobs
                .Where(j => j.IsActive && j.IsVisibleToUsers)
                .OrderByDescending(j => j.PostedDate ?? j.CreatedAt)
                .ToListAsync();

            return jobs.Select(MapToJobListing).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching jobs from database");
            return new List<JobListing>();
        }
    }

    private static JobListing MapToJobListing(Models.Entities.Job job)
    {
        // Parse Tags field for skills/level info if stored there
        return new JobListing
        {
            JobId = job.JobId ?? job.Id.ToString(),
            Title = job.Title,
            Company = job.Company,
            Level = null, // Not in simplified schema
            Salary = job.SalaryRange,
            ExperienceYears = null,
            Skills = job.Tags, // Using Tags for skills
            Source = job.Source,
            SourceId = null,
            SourceType = null,
            AllowedMode = null,
            AttributionRequired = null,
            SourceUrl = null,
            RateLimitRpm = 0,
            RateLimitBurst = 0,
            TakedownContact = null,
            TermsUrl = null,
            SourceNotes = null,
            Country = job.Country,
            City = job.City,
            WorkType = job.WorkType,
            Location = job.Location,
            ApplyUrl = job.ApplyUrl,
            Date = job.PostedDate ?? job.CreatedAt
        };
    }
}
