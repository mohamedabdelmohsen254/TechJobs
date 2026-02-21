using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using EgyptTechJobsAdmin.Data;
using EgyptTechJobsAdmin.Models.Entities;

namespace EgyptTechJobsAdmin.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        ApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<SyncController> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Sync jobs from the main EgyptTechJobsApi (CSV-based) into PostgreSQL
    /// </summary>
    [HttpPost("from-csv-api")]
    public async Task<ActionResult> SyncFromCsvApi([FromQuery] string apiUrl = "http://localhost:5200")
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{apiUrl}/api/jobs");

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new { message = $"Failed to fetch jobs from API: {response.StatusCode}" });
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<CsvApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Data == null)
            {
                return BadRequest(new { message = "No jobs data in API response" });
            }

            var imported = 0;
            var skipped = 0;
            var errors = new List<string>();

            foreach (var csvJob in apiResponse.Data)
            {
                try
                {
                    // Generate a unique JobId if missing
                    var jobId = csvJob.JobId;
                    if (string.IsNullOrEmpty(jobId))
                    {
                        jobId = GenerateJobId(csvJob.Title ?? "", csvJob.Company ?? "", csvJob.ApplyUrl ?? csvJob.SourceUrl ?? "");
                    }

                    // Check if job already exists by JobId
                    var exists = await _context.Jobs.AnyAsync(j => j.JobId == jobId);
                    if (exists)
                    {
                        skipped++;
                        continue;
                    }

                    var job = new Job
                    {
                        JobId = jobId,
                        Title = csvJob.Title ?? "Unknown",
                        Company = csvJob.Company ?? "Unknown",
                        Location = csvJob.Location,
                        Country = csvJob.Country,
                        City = csvJob.City,
                        WorkType = csvJob.WorkType,
                        ApplyUrl = csvJob.ApplyUrl ?? csvJob.SourceUrl ?? "#",
                        Source = csvJob.Source,
                        Description = csvJob.Skills,
                        SalaryRange = csvJob.Salary,
                        IsActive = true,
                        IsManualEntry = false,
                        IsVisibleToUsers = true, // Visible to users by default
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Jobs.Add(job);
                    await _context.SaveChangesAsync();
                    imported++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to import job '{csvJob.Title}': {ex.Message}");
                }
            }

            _logger.LogInformation("Synced {Imported} jobs, skipped {Skipped} duplicates", imported, skipped);

            return Ok(new
            {
                message = "Sync completed",
                imported,
                skipped,
                errors = errors.Count,
                errorDetails = errors.Take(10), // Show first 10 errors
                total = apiResponse.Data.Count
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to CSV API");
            return BadRequest(new { message = $"Failed to connect to API at the specified URL. Make sure EgyptTechJobsApi is running. Error: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sync");
            return StatusCode(500, new { message = $"Sync failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get sync status
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult> GetSyncStatus()
    {
        var totalJobs = await _context.Jobs.CountAsync();
        var importedJobs = await _context.Jobs.CountAsync(j => !j.IsManualEntry);
        var manualJobs = await _context.Jobs.CountAsync(j => j.IsManualEntry);
        var lastSync = await _context.Jobs
            .Where(j => !j.IsManualEntry)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => j.CreatedAt)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            totalJobs,
            importedJobs,
            manualJobs,
            lastSyncAt = lastSync == default ? null : (DateTime?)lastSync
        });
    }

    private static string GenerateJobId(string title, string company, string url)
    {
        var input = $"{title}_{company}_{url}_{DateTime.UtcNow.Ticks}";
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16].ToLower();
    }
}

// DTOs for CSV API response
public class CsvApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<CsvJobDto> Data { get; set; } = new();
}

public class CsvJobDto
{
    public string? JobId { get; set; }
    public string? Title { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? WorkType { get; set; }
    public string? ApplyUrl { get; set; }
    public string? SourceUrl { get; set; }
    public string? Source { get; set; }
    public string? Skills { get; set; }
    public string? Salary { get; set; }
    public string? Level { get; set; }
}
