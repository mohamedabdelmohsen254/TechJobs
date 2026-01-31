using Microsoft.AspNetCore.Mvc;
using EgyptTechJobsApi.Services;

namespace EgyptTechJobsApi.Controllers
{
    /// <summary>
    /// Controller for fetching jobs from external sources
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FetchController : ControllerBase
    {
        private readonly JobFetchService _fetchService;

        public FetchController()
        {
            _fetchService = new JobFetchService();
        }

        /// <summary>
        /// Fetch jobs from all enabled sources and save to CSV
        /// </summary>
        /// <param name="options">Options to control which sources to fetch from</param>
        /// <returns>Summary of the fetch operation</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/fetch
        ///     {
        ///         "fetchGreenhouse": true,
        ///         "fetchLever": true,
        ///         "fetchWorkable": true,
        ///         "fetchJooble": true,
        ///         "fetchRemoteOk": true,
        ///         "fetchRemotive": true,
        ///         "fetchHimalayas": true,
        ///         "fetchJobicy": true,
        ///         "joobleMaxPages": 3
        ///     }
        ///     
        /// All options default to true if not specified.
        /// </remarks>
        /// <response code="200">Returns the fetch result summary</response>
        /// <response code="500">If an error occurs during fetching</response>
        [HttpPost]
        [ProducesResponseType(typeof(FetchResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FetchResult>> FetchJobs([FromBody] FetchOptions? options = null)
        {
            try
            {
                options ??= new FetchOptions();
                var result = await _fetchService.FetchAndSaveJobsAsync(options);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = result.Success,
                        message = $"Successfully fetched and saved {result.SavedToCsv} jobs",
                        startTime = result.StartTime,
                        endTime = result.EndTime,
                        durationSeconds = result.Duration.TotalSeconds,
                        totalFetched = result.TotalFetched,
                        afterDeduplication = result.AfterDedup,
                        savedToCsv = result.SavedToCsv,
                        sourceStats = result.SourceStats
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        error = result.Error
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Fetch jobs from a specific source only
        /// </summary>
        /// <param name="source">The source to fetch from (greenhouse, lever, workable, jooble, remoteok, remotive, himalayas, jobicy)</param>
        /// <returns>Summary of the fetch operation</returns>
        [HttpPost("{source}")]
        [ProducesResponseType(typeof(FetchResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FetchResult>> FetchFromSource(string source)
        {
            var options = new FetchOptions
            {
                FetchGreenhouse = false,
                FetchLever = false,
                FetchWorkable = false,
                FetchJooble = false,
                FetchRemoteOk = false,
                FetchRemotive = false,
                FetchHimalayas = false,
                FetchJobicy = false
            };

            switch (source.ToLower())
            {
                case "greenhouse":
                    options.FetchGreenhouse = true;
                    break;
                case "lever":
                    options.FetchLever = true;
                    break;
                case "workable":
                    options.FetchWorkable = true;
                    break;
                case "jooble":
                    options.FetchJooble = true;
                    break;
                case "remoteok":
                    options.FetchRemoteOk = true;
                    break;
                case "remotive":
                    options.FetchRemotive = true;
                    break;
                case "himalayas":
                    options.FetchHimalayas = true;
                    break;
                case "jobicy":
                    options.FetchJobicy = true;
                    break;
                default:
                    return BadRequest(new
                    {
                        error = $"Unknown source: {source}",
                        validSources = new[] { "greenhouse", "lever", "workable", "jooble", "remoteok", "remotive", "himalayas", "jobicy" }
                    });
            }

            return await FetchJobs(options);
        }

        /// <summary>
        /// Get list of available sources
        /// </summary>
        [HttpGet("sources")]
        public ActionResult GetSources()
        {
            return Ok(new
            {
                sources = new[]
                {
                    new { id = "greenhouse", name = "Greenhouse", description = "Official API for Greenhouse job boards", rateLimit = "60 RPM" },
                    new { id = "lever", name = "Lever", description = "Official API for Lever job boards", rateLimit = "60 RPM" },
                    new { id = "workable", name = "Workable", description = "Official API for Workable job boards", rateLimit = "60 RPM" },
                    new { id = "jooble", name = "Jooble", description = "Partner API for job aggregation", rateLimit = "30 RPM" },
                    new { id = "remoteok", name = "RemoteOK", description = "Public API for remote jobs", rateLimit = "10 RPM" },
                    new { id = "remotive", name = "Remotive", description = "Public API for remote jobs", rateLimit = "30 RPM" },
                    new { id = "himalayas", name = "Himalayas", description = "API for remote tech jobs", rateLimit = "20 RPM" },
                    new { id = "jobicy", name = "Jobicy", description = "API for remote jobs", rateLimit = "20 RPM" }
                }
            });
        }
    }
}
