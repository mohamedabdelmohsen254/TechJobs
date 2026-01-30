using Microsoft.AspNetCore.Mvc;
using EgyptTechJobsApi.Models;
using EgyptTechJobsApi.Services;

namespace EgyptTechJobsApi.Controllers
{
    /// <summary>
    /// Jobs API Controller - Get and filter job listings
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class JobsController : ControllerBase
    {
        private readonly JobService _jobService;
        private readonly ILogger<JobsController> _logger;

        public JobsController(JobService jobService, ILogger<JobsController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        /// <summary>
        /// Get all jobs (with optional filtering)
        /// </summary>
        /// <param name="title">Filter by job title</param>
        /// <param name="company">Filter by company name</param>
        /// <param name="city">Filter by city</param>
        /// <param name="level">Filter by experience level</param>
        /// <param name="source">Filter by job source</param>
        /// <param name="workType">Filter by work type (Remote, On-site, Hybrid)</param>
        /// <returns>List of jobs matching criteria</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<JobListing>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<JobListing>>>> GetJobs(
            [FromQuery] string title = null,
            [FromQuery] string company = null,
            [FromQuery] string city = null,
            [FromQuery] string level = null,
            [FromQuery] string source = null,
            [FromQuery] string workType = null)
        {
            try
            {
                var filter = new JobFilterOptions
                {
                    Title = title,
                    Company = company,
                    City = city,
                    Level = level,
                    Source = source,
                    WorkType = workType
                };

                var jobs = await _jobService.GetJobsAsync(filter);

                return Ok(new ApiResponse<List<JobListing>>
                {
                    Success = true,
                    Message = $"Found {jobs.Count} jobs",
                    Data = jobs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs");
                return StatusCode(500, new ApiResponse<List<JobListing>>
                {
                    Success = false,
                    Message = "Error retrieving jobs"
                });
            }
        }

        /// <summary>
        /// Get jobs with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 50)</param>
        /// <param name="title">Filter by job title</param>
        /// <param name="company">Filter by company name</param>
        /// <returns>Paginated list of jobs</returns>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<dynamic>>> GetPagedJobs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string title = null,
            [FromQuery] string company = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 500) pageSize = 50;

                var filter = new JobFilterOptions
                {
                    Title = title,
                    Company = company,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var (jobs, total) = await _jobService.GetPagedJobsAsync(filter);

                return Ok(new ApiResponse<dynamic>
                {
                    Success = true,
                    Message = $"Page {pageNumber} of {Math.Ceiling((double)total / pageSize)}",
                    Data = new
                    {
                        items = jobs,
                        totalCount = total,
                        pageNumber = pageNumber,
                        pageSize = pageSize,
                        totalPages = Math.Ceiling((double)total / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged jobs");
                return StatusCode(500, new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Error retrieving jobs"
                });
            }
        }

        /// <summary>
        /// Search jobs by keyword
        /// </summary>
        /// <param name="keyword">Search keyword (searches title, company, location, skills)</param>
        /// <returns>Jobs matching the search keyword</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<List<JobListing>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<JobListing>>>> SearchJobs([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new ApiResponse<List<JobListing>>
                    {
                        Success = false,
                        Message = "Keyword is required"
                    });
                }

                var jobs = await _jobService.SearchJobsAsync(keyword);

                return Ok(new ApiResponse<List<JobListing>>
                {
                    Success = true,
                    Message = $"Found {jobs.Count} jobs matching '{keyword}'",
                    Data = jobs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching jobs");
                return StatusCode(500, new ApiResponse<List<JobListing>>
                {
                    Success = false,
                    Message = "Error searching jobs"
                });
            }
        }

        /// <summary>
        /// Get jobs by city
        /// </summary>
        /// <param name="city">City name</param>
        /// <returns>Jobs in the specified city</returns>
        [HttpGet("by-city/{city}")]
        [ProducesResponseType(typeof(ApiResponse<List<JobListing>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<JobListing>>>> GetJobsByCity(string city)
        {
            try
            {
                var jobs = await _jobService.GetJobsByCityAsync(city);

                return Ok(new ApiResponse<List<JobListing>>
                {
                    Success = true,
                    Message = $"Found {jobs.Count} jobs in {city}",
                    Data = jobs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by city");
                return StatusCode(500, new ApiResponse<List<JobListing>>
                {
                    Success = false,
                    Message = "Error retrieving jobs"
                });
            }
        }

        /// <summary>
        /// Get jobs by company
        /// </summary>
        /// <param name="company">Company name</param>
        /// <returns>Jobs from the specified company</returns>
        [HttpGet("by-company/{company}")]
        [ProducesResponseType(typeof(ApiResponse<List<JobListing>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<JobListing>>>> GetJobsByCompany(string company)
        {
            try
            {
                var jobs = await _jobService.GetJobsByCompanyAsync(company);

                return Ok(new ApiResponse<List<JobListing>>
                {
                    Success = true,
                    Message = $"Found {jobs.Count} jobs from {company}",
                    Data = jobs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs by company");
                return StatusCode(500, new ApiResponse<List<JobListing>>
                {
                    Success = false,
                    Message = "Error retrieving jobs"
                });
            }
        }

        /// <summary>
        /// Get count of jobs
        /// </summary>
        /// <returns>Total number of jobs</returns>
        [HttpGet("count")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> GetJobCount()
        {
            try
            {
                var jobs = await _jobService.GetJobsAsync();
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = "Job count retrieved",
                    Data = jobs.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job count");
                return StatusCode(500, new ApiResponse<int>
                {
                    Success = false,
                    Message = "Error retrieving count"
                });
            }
        }

        /// <summary>
        /// Get a random job
        /// </summary>
        /// <returns>A random job from the database</returns>
        [HttpGet("random")]
        [ProducesResponseType(typeof(ApiResponse<JobListing>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<JobListing>>> GetRandomJob()
        {
            try
            {
                var jobs = await _jobService.GetJobsAsync();
                if (jobs.Count == 0)
                {
                    return NotFound(new ApiResponse<JobListing>
                    {
                        Success = false,
                        Message = "No jobs found"
                    });
                }

                var random = new Random();
                var randomJob = jobs[random.Next(jobs.Count)];

                return Ok(new ApiResponse<JobListing>
                {
                    Success = true,
                    Message = "Random job retrieved",
                    Data = randomJob
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random job");
                return StatusCode(500, new ApiResponse<JobListing>
                {
                    Success = false,
                    Message = "Error retrieving random job"
                });
            }
        }
    }
}
