using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using EgyptTechJobsApi.Models;
using System.Globalization;

namespace EgyptTechJobsApi.Services
{
    /// <summary>
    /// Service for reading job data from CSV
    /// </summary>
    public class JobService
    {
        private readonly string _csvPath;
        private List<JobListing> _cachedJobs = new List<JobListing>();
        private DateTime _cacheTime;

        public JobService(IConfiguration configuration)
        {
            // Use absolute path to the data folder
            _csvPath = @"e:\selfDevelopment\TechJobs\data\Egypt_Tech_Jobs.csv";
        }

        /// <summary>
        /// Get all jobs with optional filtering
        /// </summary>
        public async Task<List<JobListing>> GetJobsAsync(JobFilterOptions filter = null)
        {
            var jobs = await ReadJobsAsync();

            if (filter != null)
            {
                jobs = FilterJobs(jobs, filter);
            }

            return jobs;
        }

        /// <summary>
        /// Get paginated jobs
        /// </summary>
        public async Task<(List<JobListing> jobs, int total)> GetPagedJobsAsync(JobFilterOptions filter = null)
        {
            var jobs = await GetJobsAsync(filter);
            var total = jobs.Count;

            if (filter != null && filter.PageSize > 0)
            {
                jobs = jobs
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();
            }

            return (jobs, total);
        }

        /// <summary>
        /// Get job statistics
        /// </summary>
        public async Task<JobStatistics> GetStatisticsAsync()
        {
            var jobs = await ReadJobsAsync();

            return new JobStatistics
            {
                TotalJobs = jobs.Count,
                UniqueCompanies = jobs.Select(j => j.Company).Distinct().Count(),
                UniqueCities = jobs.Select(j => j.City).Distinct().Count(),
                SourceBreakdown = jobs
                    .GroupBy(j => j.Source)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                LevelBreakdown = jobs
                    .GroupBy(j => j.Level)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                WorkTypeBreakdown = jobs
                    .GroupBy(j => j.WorkType)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count())
            };
        }

        /// <summary>
        /// Search jobs by keyword
        /// </summary>
        public async Task<List<JobListing>> SearchJobsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<JobListing>();

            var jobs = await ReadJobsAsync();
            var searchTerm = keyword.ToLower();

            return jobs.Where(j =>
                (j.Title?.ToLower().Contains(searchTerm) ?? false) ||
                (j.Company?.ToLower().Contains(searchTerm) ?? false) ||
                (j.Location?.ToLower().Contains(searchTerm) ?? false) ||
                (j.Skills?.ToLower().Contains(searchTerm) ?? false)
            ).ToList();
        }

        /// <summary>
        /// Get jobs by city
        /// </summary>
        public async Task<List<JobListing>> GetJobsByCityAsync(string city)
        {
            var jobs = await ReadJobsAsync();
            return jobs.Where(j => j.City?.Equals(city, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }

        /// <summary>
        /// Get jobs by company
        /// </summary>
        public async Task<List<JobListing>> GetJobsByCompanyAsync(string company)
        {
            var jobs = await ReadJobsAsync();
            return jobs.Where(j => j.Company?.Equals(company, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }

        /// <summary>
        /// Get unique values for a field
        /// </summary>
        public async Task<List<string>> GetUniqueValuesAsync(string fieldName)
        {
            var jobs = await ReadJobsAsync();

            return fieldName.ToLower() switch
            {
                "city" => jobs.Select(j => j.City).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
                "company" => jobs.Select(j => j.Company).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
                "source" => jobs.Select(j => j.Source).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
                "level" => jobs.Select(j => j.Level).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
                "worktype" => jobs.Select(j => j.WorkType).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x).ToList(),
                _ => new List<string>()
            };
        }

        /// <summary>
        /// Read jobs from CSV with caching
        /// </summary>
        private async Task<List<JobListing>> ReadJobsAsync()
        {
            // Use cache if available and less than 5 minutes old
            if (_cachedJobs != null && DateTime.UtcNow - _cacheTime < TimeSpan.FromMinutes(5))
            {
                return _cachedJobs;
            }

            return await Task.Run(() =>
            {
                var jobs = new List<JobListing>();

                if (!File.Exists(_csvPath))
                {
                    return jobs;
                }

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    BadDataFound = null,
                };

                try
                {
                    using (var reader = new StreamReader(_csvPath))
                    using (var csv = new CsvReader(reader, config))
                    {
                        jobs = csv.GetRecords<JobListing>().ToList();
                    }

                    _cachedJobs = jobs;
                    _cacheTime = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    // Log error but don't throw
                    System.Diagnostics.Debug.WriteLine($"Error reading CSV: {ex.Message}");
                }

                return jobs;
            });
        }

        /// <summary>
        /// Filter jobs based on options
        /// </summary>
        private List<JobListing> FilterJobs(List<JobListing> jobs, JobFilterOptions filter)
        {
            var result = jobs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.Title))
                result = result.Where(j => j.Title?.Contains(filter.Title, StringComparison.OrdinalIgnoreCase) ?? false);

            if (!string.IsNullOrWhiteSpace(filter.Company))
                result = result.Where(j => j.Company?.Contains(filter.Company, StringComparison.OrdinalIgnoreCase) ?? false);

            if (!string.IsNullOrWhiteSpace(filter.City))
                result = result.Where(j => j.City?.Equals(filter.City, StringComparison.OrdinalIgnoreCase) ?? false);

            if (!string.IsNullOrWhiteSpace(filter.Level))
                result = result.Where(j => j.Level?.Equals(filter.Level, StringComparison.OrdinalIgnoreCase) ?? false);

            if (!string.IsNullOrWhiteSpace(filter.Source))
                result = result.Where(j => j.Source?.Equals(filter.Source, StringComparison.OrdinalIgnoreCase) ?? false);

            if (!string.IsNullOrWhiteSpace(filter.WorkType))
                result = result.Where(j => j.WorkType?.Equals(filter.WorkType, StringComparison.OrdinalIgnoreCase) ?? false);

            return result.ToList();
        }
    }
}
