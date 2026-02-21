using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EgyptTechJobsAdmin.Models.DTOs;
using EgyptTechJobsAdmin.Services;

namespace EgyptTechJobsAdmin.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>
    /// Get paginated list of jobs with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<JobResponseDto>>> GetJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? country = null,
        [FromQuery] string? workType = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _jobService.GetJobsAsync(page, pageSize, search, country, workType, isActive);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific job by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<JobResponseDto>> GetJob(int id)
    {
        var job = await _jobService.GetJobByIdAsync(id);
        if (job == null)
            return NotFound(new { message = $"Job with ID {id} not found" });

        return Ok(job);
    }

    /// <summary>
    /// Create a new job manually
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<JobResponseDto>> CreateJob([FromBody] CreateJobDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var job = await _jobService.CreateJobAsync(dto);
        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
    }

    /// <summary>
    /// Update an existing job
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<JobResponseDto>> UpdateJob(int id, [FromBody] UpdateJobDto dto)
    {
        var job = await _jobService.UpdateJobAsync(id, dto);
        if (job == null)
            return NotFound(new { message = $"Job with ID {id} not found" });

        return Ok(job);
    }

    /// <summary>
    /// Delete a job
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteJob(int id)
    {
        var deleted = await _jobService.DeleteJobAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Job with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Bulk create jobs (for importing)
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<List<JobResponseDto>>> BulkCreateJobs([FromBody] List<CreateJobDto> dtos)
    {
        var results = new List<JobResponseDto>();
        foreach (var dto in dtos)
        {
            var job = await _jobService.CreateJobAsync(dto);
            results.Add(job);
        }

        return Ok(new { created = results.Count, jobs = results });
    }

    /// <summary>
    /// Toggle job visibility for users
    /// </summary>
    [HttpPatch("{id}/visibility")]
    public async Task<ActionResult<JobResponseDto>> ToggleVisibility(int id, [FromQuery] bool visible)
    {
        var job = await _jobService.UpdateJobAsync(id, new UpdateJobDto { IsVisibleToUsers = visible });
        if (job == null)
            return NotFound(new { message = $"Job with ID {id} not found" });

        return Ok(job);
    }

    /// <summary>
    /// Bulk toggle visibility for multiple jobs
    /// </summary>
    [HttpPatch("bulk-visibility")]
    public async Task<ActionResult> BulkToggleVisibility([FromBody] BulkVisibilityDto dto)
    {
        var updated = 0;
        foreach (var id in dto.JobIds)
        {
            var result = await _jobService.UpdateJobAsync(id, new UpdateJobDto { IsVisibleToUsers = dto.Visible });
            if (result != null) updated++;
        }

        return Ok(new { message = $"Updated visibility for {updated} jobs", updated });
    }
}

public class BulkVisibilityDto
{
    public List<int> JobIds { get; set; } = new();
    public bool Visible { get; set; }
}
