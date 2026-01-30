using Microsoft.AspNetCore.Mvc;
using EgyptTechJobsApi.Models;
using EgyptTechJobsApi.Services;

namespace EgyptTechJobsApi.Controllers
{
    /// <summary>
    /// Statistics API Controller - Get job statistics and analytics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class StatisticsController : ControllerBase
    {
        private readonly JobService _jobService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(JobService jobService, ILogger<StatisticsController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        /// <summary>
        /// Get job statistics (total, companies, cities, breakdowns)
        /// </summary>
        /// <returns>Job statistics including counts and breakdowns</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<JobStatistics>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<JobStatistics>>> GetStatistics()
        {
            try
            {
                var stats = await _jobService.GetStatisticsAsync();

                return Ok(new ApiResponse<JobStatistics>
                {
                    Success = true,
                    Message = "Statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new ApiResponse<JobStatistics>
                {
                    Success = false,
                    Message = "Error retrieving statistics"
                });
            }
        }

        /// <summary>
        /// Get unique cities
        /// </summary>
        /// <returns>List of unique cities</returns>
        [HttpGet("cities")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetCities()
        {
            try
            {
                var cities = await _jobService.GetUniqueValuesAsync("city");

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = $"Found {cities.Count} cities",
                    Data = cities
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cities");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Error retrieving cities"
                });
            }
        }

        /// <summary>
        /// Get unique companies
        /// </summary>
        /// <returns>List of unique companies</returns>
        [HttpGet("companies")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetCompanies()
        {
            try
            {
                var companies = await _jobService.GetUniqueValuesAsync("company");

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = $"Found {companies.Count} companies",
                    Data = companies
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting companies");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Error retrieving companies"
                });
            }
        }

        /// <summary>
        /// Get unique job sources
        /// </summary>
        /// <returns>List of unique job sources</returns>
        [HttpGet("sources")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetSources()
        {
            try
            {
                var sources = await _jobService.GetUniqueValuesAsync("source");

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = $"Found {sources.Count} sources",
                    Data = sources
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sources");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Error retrieving sources"
                });
            }
        }

        /// <summary>
        /// Get unique job levels
        /// </summary>
        /// <returns>List of unique job levels</returns>
        [HttpGet("levels")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetLevels()
        {
            try
            {
                var levels = await _jobService.GetUniqueValuesAsync("level");

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = $"Found {levels.Count} levels",
                    Data = levels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting levels");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Error retrieving levels"
                });
            }
        }

        /// <summary>
        /// Get unique work types
        /// </summary>
        /// <returns>List of unique work types</returns>
        [HttpGet("work-types")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetWorkTypes()
        {
            try
            {
                var workTypes = await _jobService.GetUniqueValuesAsync("worktype");

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = $"Found {workTypes.Count} work types",
                    Data = workTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work types");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Error retrieving work types"
                });
            }
        }
    }
}
