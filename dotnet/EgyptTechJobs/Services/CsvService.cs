using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using EgyptTechJobs.Models;
using System.Globalization;

namespace EgyptTechJobs.Services
{
    /// <summary>
    /// Service for reading and writing CSV files containing job listings
    /// </summary>
    public class CsvService
    {
        private readonly string _outputPath;

        public CsvService(string outputPath = null)
        {
            _outputPath = outputPath ?? Path.Combine(Directory.GetCurrentDirectory(), "Egypt_Tech_Jobs.csv");
        }

        /// <summary>
        /// Writes job listings to a CSV file
        /// </summary>
        public async Task WriteJobsAsync(List<JobListing> jobs)
        {
            await Task.Run(() =>
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                };

                using (var writer = new StreamWriter(_outputPath, false))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteRecords(jobs);
                }
            });
        }

        /// <summary>
        /// Reads job listings from a CSV file
        /// </summary>
        public async Task<List<JobListing>> ReadJobsAsync()
        {
            return await Task.Run(() =>
            {
                var jobs = new List<JobListing>();

                if (!File.Exists(_outputPath))
                {
                    return jobs;
                }

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                };

                using (var reader = new StreamReader(_outputPath))
                using (var csv = new CsvReader(reader, config))
                {
                    jobs = csv.GetRecords<JobListing>().ToList();
                }

                return jobs;
            });
        }

        /// <summary>
        /// Filters jobs based on criteria
        /// </summary>
        public List<JobListing> FilterJobs(List<JobListing> jobs, Func<JobListing, bool> filter)
        {
            return jobs.Where(filter).ToList();
        }

        /// <summary>
        /// Gets statistics about the jobs
        /// </summary>
        public JobStatistics GetStatistics(List<JobListing> jobs)
        {
            return new JobStatistics
            {
                TotalJobs = jobs.Count,
                UniqueCompanies = jobs.Select(j => j.Company).Distinct().Count(),
                UniqueCities = jobs.Select(j => j.City).Distinct().Count(),
                SourceBreakdown = jobs
                    .GroupBy(j => j.Source)
                    .ToDictionary(g => g.Key, g => g.Count()),
                LevelBreakdown = jobs
                    .GroupBy(j => j.Level)
                    .ToDictionary(g => g.Key, g => g.Count()),
                WorkTypeBreakdown = jobs
                    .GroupBy(j => j.WorkType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }
    }

    public class JobStatistics
    {
        public int TotalJobs { get; set; }
        public int UniqueCompanies { get; set; }
        public int UniqueCities { get; set; }
        public Dictionary<string, int> SourceBreakdown { get; set; }
        public Dictionary<string, int> LevelBreakdown { get; set; }
        public Dictionary<string, int> WorkTypeBreakdown { get; set; }
    }
}
