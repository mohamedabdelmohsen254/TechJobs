using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EgyptTechJobsAdmin.Services;

namespace EgyptTechJobsAdmin.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IJobService _jobService;

    public DashboardController(IJobService jobService)
    {
        _jobService = jobService;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var stats = await _jobService.GetDashboardStatsAsync();
        return Ok(stats);
    }
}
