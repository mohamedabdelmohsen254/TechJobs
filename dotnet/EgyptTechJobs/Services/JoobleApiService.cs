using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EgyptTechJobs.Config;
using EgyptTechJobs.Models;

namespace EgyptTechJobs.Services
{
    /// <summary>
    /// Service for fetching jobs from Jooble API
    /// </summary>
    public class JoobleApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _settings;
        private const string BaseUrl = "https://api.jooble.org/api/v1/vacancies";

        public JoobleApiService(HttpClient httpClient, AppSettings settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Fetches jobs from Jooble API for a given keyword
        /// </summary>
        public async Task<List<JobListing>> FetchJobsByKeywordAsync(string keyword)
        {
            var jobs = new List<JobListing>();

            if (!_settings.JoobleEnabled)
            {
                return jobs;
            }

            try
            {
                for (int page = 1; page <= _settings.JoobleMaxPages; page++)
                {
                    var payload = new
                    {
                        keywords = new[] { keyword },
                        location = "Egypt",
                        datePosted = _settings.JoobleDaysBack,
                        pageNumber = page
                    };

                    var content = new StringContent(
                        JsonSerializer.Serialize(payload),
                        System.Text.Encoding.UTF8,
                        "application/json");

                    var response = await _httpClient.PostAsync(
                        $"{BaseUrl}/{_settings.JoobleApiKey}",
                        content);

                    if (!response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                    if (result.TryGetProperty("jobs", out var jobsArray))
                    {
                        foreach (var jobElement in jobsArray.EnumerateArray())
                        {
                            var job = ParseJobFromJooble(jobElement);
                            if (job != null)
                            {
                                jobs.Add(job);
                            }
                        }
                    }

                    // Check if there are more results
                    if (result.TryGetProperty("totalCount", out var totalCount))
                    {
                        if (jobs.Count >= totalCount.GetInt32())
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching from Jooble for keyword '{keyword}': {ex.Message}");
            }

            return jobs;
        }

        private JobListing ParseJobFromJooble(JsonElement jobElement)
        {
            try
            {
                var job = new JobListing
                {
                    JobId = GenerateJobId(),
                    Source = "Jooble",
                    SourceId = "jooble",
                    SourceType = "official_api",
                    AllowedMode = "full_display",
                    AttributionRequired = "No",
                    SourceUrl = "https://jooble.org",
                    RateLimitRpm = 60,
                    RateLimitBurst = 10,
                    Country = "Egypt",
                    Date = DateTime.UtcNow
                };

                if (jobElement.TryGetProperty("title", out var title))
                    job.Title = title.GetString();

                if (jobElement.TryGetProperty("company", out var company))
                    job.Company = company.GetString();

                if (jobElement.TryGetProperty("location", out var location))
                    job.Location = location.GetString();

                if (jobElement.TryGetProperty("link", out var link))
                    job.ApplyUrl = link.GetString();

                return job;
            }
            catch
            {
                return null;
            }
        }

        private string GenerateJobId()
        {
            return unchecked((long)HashCode.Combine(DateTime.UtcNow, Random.Shared.Next())).ToString();
        }
    }
}
