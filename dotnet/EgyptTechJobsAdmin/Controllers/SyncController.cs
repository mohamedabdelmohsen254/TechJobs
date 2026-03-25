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
            client.Timeout = TimeSpan.FromMinutes(2);
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

            var existingJobIdList = await _context.Jobs
                .Select(j => j.JobId)
                .Where(id => id != null)
                .ToListAsync();
            var existingJobIds = existingJobIdList.ToHashSet();

            var jobsToInsert = new List<Job>();

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
                    if (existingJobIds.Contains(jobId))
                    {
                        skipped++;
                        continue;
                    }

                    var job = new Job
                    {
                        JobId = jobId,
                        Title = Truncate(csvJob.Title ?? "Unknown", 500),
                        Company = Truncate(csvJob.Company ?? "Unknown", 200),
                        Location = Truncate(csvJob.Location, 200),
                        Country = Truncate(csvJob.Country, 100),
                        City = Truncate(csvJob.City, 100),
                        WorkType = Truncate(csvJob.WorkType, 50),
                        ApplyUrl = Truncate(csvJob.ApplyUrl ?? csvJob.SourceUrl ?? "#", 1000),
                        Source = Truncate(csvJob.Source, 100),
                        Description = Truncate(csvJob.Skills, 5000),
                        SalaryRange = Truncate(csvJob.Salary, 500),
                        IsActive = true,
                        IsManualEntry = false,
                        IsVisibleToUsers = true, // Visible to users by default
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    jobsToInsert.Add(job);
                    existingJobIds.Add(jobId);
                    imported++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to import job '{csvJob.Title}': {ex.Message}");
                }
            }

            if (jobsToInsert.Count > 0)
            {
                const int batchSize = 500;
                foreach (var batch in jobsToInsert.Chunk(batchSize))
                {
                    try
                    {
                        _context.Jobs.AddRange(batch);
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();
                    }
                    catch (Exception batchEx)
                    {
                        _logger.LogWarning(batchEx, "Batch insert failed; falling back to per-row inserts for this batch.");
                        _context.ChangeTracker.Clear();

                        foreach (var job in batch)
                        {
                            try
                            {
                                _context.Jobs.Add(job);
                                await _context.SaveChangesAsync();
                                _context.ChangeTracker.Clear();
                            }
                            catch (Exception rowEx)
                            {
                                errors.Add($"Failed to import job '{job.Title}': {rowEx.Message}");
                                _context.ChangeTracker.Clear();
                            }
                        }
                    }
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

    /// <summary>
    /// Fetch jobs from external sources via main API and sync to database
    /// </summary>
    [HttpPost("fetch-and-sync")]
    public async Task<ActionResult> FetchAndSync([FromQuery] string apiUrl = "http://localhost:5200", [FromBody] FetchOptionsDto? options = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5); // Fetch can take a while

            // Step 1: Trigger fetch from external sources
            _logger.LogInformation("Starting fetch from external sources via {ApiUrl}", apiUrl);
            
            var fetchOptions = options ?? new FetchOptionsDto();
            var fetchContent = new StringContent(
                JsonSerializer.Serialize(fetchOptions),
                System.Text.Encoding.UTF8,
                "application/json"
            );
            
            var fetchResponse = await client.PostAsync($"{apiUrl}/api/fetch", fetchContent);
            
            if (!fetchResponse.IsSuccessStatusCode)
            {
                var errorContent = await fetchResponse.Content.ReadAsStringAsync();
                _logger.LogError("Fetch failed: {Error}", errorContent);
                return BadRequest(new { message = $"Fetch from external sources failed: {fetchResponse.StatusCode}", details = errorContent });
            }

            var fetchResult = await fetchResponse.Content.ReadAsStringAsync();
            var fetchData = JsonSerializer.Deserialize<FetchResultDto>(fetchResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Fetch completed: {TotalFetched} fetched, {Saved} saved to main API", 
                fetchData?.TotalFetched ?? 0, fetchData?.SavedToDb ?? 0);

            // Step 2: Sync from main API to admin database
            _logger.LogInformation("Starting sync from main API to admin database");
            
            var jobsResponse = await client.GetAsync($"{apiUrl}/api/jobs");
            
            if (!jobsResponse.IsSuccessStatusCode)
            {
                return BadRequest(new { message = $"Failed to get jobs from API: {jobsResponse.StatusCode}" });
            }

            var json = await jobsResponse.Content.ReadAsStringAsync();
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

            var existingJobIdList = await _context.Jobs
                .Select(j => j.JobId)
                .Where(id => id != null)
                .ToListAsync();
            var existingJobIds = existingJobIdList.ToHashSet();

            var jobsToInsert = new List<Job>();

            foreach (var csvJob in apiResponse.Data)
            {
                var jobId = csvJob.JobId;
                if (string.IsNullOrEmpty(jobId))
                {
                    jobId = GenerateJobId(csvJob.Title ?? "", csvJob.Company ?? "", csvJob.ApplyUrl ?? csvJob.SourceUrl ?? "");
                }

                if (existingJobIds.Contains(jobId))
                {
                    skipped++;
                    continue;
                }

                var job = new Job
                {
                    JobId = jobId,
                    Title = Truncate(csvJob.Title ?? "Unknown", 500),
                    Company = Truncate(csvJob.Company ?? "Unknown", 200),
                    Location = Truncate(csvJob.Location, 200),
                    Country = Truncate(csvJob.Country, 100),
                    City = Truncate(csvJob.City, 100),
                    WorkType = Truncate(csvJob.WorkType, 50),
                    ApplyUrl = Truncate(csvJob.ApplyUrl ?? csvJob.SourceUrl ?? "#", 1000),
                    Source = Truncate(csvJob.Source, 100),
                    Description = Truncate(csvJob.Skills, 5000),
                    SalaryRange = Truncate(csvJob.Salary, 500),
                    IsActive = true,
                    IsManualEntry = false,
                    IsVisibleToUsers = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                jobsToInsert.Add(job);
                existingJobIds.Add(jobId);
                imported++;
            }

            if (jobsToInsert.Count > 0)
            {
                const int batchSize = 500;
                foreach (var batch in jobsToInsert.Chunk(batchSize))
                {
                    _context.Jobs.AddRange(batch);
                    await _context.SaveChangesAsync();
                    _context.ChangeTracker.Clear();
                }
            }

            _logger.LogInformation("Fetch and sync completed: fetched {TotalFetched}, imported {Imported}, skipped {Skipped}", 
                fetchData?.TotalFetched ?? 0, imported, skipped);

            return Ok(new
            {
                message = "Fetch and sync completed successfully",
                fetchResult = new
                {
                    totalFetched = fetchData?.TotalFetched ?? 0,
                    afterDeduplication = fetchData?.AfterDeduplication ?? 0,
                    savedToMainApi = fetchData?.SavedToDb ?? 0,
                    durationSeconds = fetchData?.DurationSeconds ?? 0,
                    sourceStats = fetchData?.SourceStats
                },
                syncResult = new
                {
                    imported,
                    skipped,
                    total = apiResponse.Data.Count
                }
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to main API");
            return BadRequest(new { message = $"Failed to connect to API. Make sure EgyptTechJobsApi is running at the specified URL. Error: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during fetch and sync");
            return StatusCode(500, new { message = $"Fetch and sync failed: {ex.Message}" });
        }
    }

    private static string GenerateJobId(string title, string company, string url)
    {
        var input = $"{title}_{company}_{url}_{DateTime.UtcNow.Ticks}";
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16].ToLower();
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
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

public class FetchOptionsDto
{
    public bool FetchGreenhouse { get; set; } = true;
    public bool FetchLever { get; set; } = true;
    public bool FetchWorkable { get; set; } = true;
    public bool FetchJooble { get; set; } = true;
    public bool FetchRemoteOk { get; set; } = true;
    public bool FetchRemotive { get; set; } = true;
    public bool FetchHimalayas { get; set; } = true;
    public bool FetchJobicy { get; set; } = true;
    public int JoobleMaxPages { get; set; } = 3;
}

public class FetchResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int TotalFetched { get; set; }
    public int AfterDeduplication { get; set; }
    public int SavedToDb { get; set; }
    public double DurationSeconds { get; set; }
    public Dictionary<string, int>? SourceStats { get; set; }
}
