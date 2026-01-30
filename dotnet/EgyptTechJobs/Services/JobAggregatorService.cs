using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EgyptTechJobs.Config;
using EgyptTechJobs.Models;

namespace EgyptTechJobs.Services
{
    /// <summary>
    /// Service for aggregating and processing job listings
    /// </summary>
    public class JobAggregatorService
    {
        private readonly AppSettings _settings;
        private readonly List<JobListing> _allJobs;

        public JobAggregatorService(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _allJobs = new List<JobListing>();
        }

        /// <summary>
        /// Adds a job to the aggregator
        /// </summary>
        public void AddJob(JobListing job)
        {
            if (job != null)
            {
                _allJobs.Add(job);
            }
        }

        /// <summary>
        /// Adds multiple jobs to the aggregator
        /// </summary>
        public void AddJobs(IEnumerable<JobListing> jobs)
        {
            if (jobs != null)
            {
                _allJobs.AddRange(jobs);
            }
        }

        /// <summary>
        /// Filters jobs based on application settings
        /// </summary>
        public List<JobListing> GetFilteredJobs()
        {
            var filtered = _allJobs.AsEnumerable();

            // Filter by country
            if (_settings.EgyptOnly)
            {
                filtered = filtered.Where(j => j.Country?.Equals("Egypt", StringComparison.OrdinalIgnoreCase) ?? false);
            }

            // Filter by technology jobs
            if (_settings.TechOnly)
            {
                filtered = filtered.Where(j => IsTechJob(j));
            }

            // Filter by product/design roles
            if (!_settings.IncludeProduct)
            {
                filtered = filtered.Where(j => !IsProductRole(j.Title));
            }

            if (!_settings.IncludeDesign)
            {
                filtered = filtered.Where(j => !IsDesignRole(j.Title));
            }

            // Remove duplicates based on title and company
            filtered = filtered
                .GroupBy(j => new { j.Title, j.Company })
                .Select(g => g.First());

            return filtered.OrderByDescending(j => j.Date).ToList();
        }

        /// <summary>
        /// Determines if a job is a tech job
        /// </summary>
        private bool IsTechJob(JobListing job)
        {
            if (job?.Title == null)
                return false;

            var title = job.Title.ToLower();

            var techKeywords = new[]
            {
                "developer", "engineer", "programmer", "analyst", "architect",
                "sql", "database", "oracle", "python", "java", "c#", "javascript",
                "frontend", "backend", "full-stack", "fullstack", "devops", "cloud",
                "aws", "azure", "kubernetes", "docker", "react", "node", "nodejs",
                "qa", "quality assurance", "tester", "automation", "sqa",
                "data", "big data", "hadoop", "spark", "etl", "business intelligence",
                "bi developer", "power bi", "tableau", "network", "infrastructure",
                "security", "cybersecurity", "system", "admin"
            };

            return techKeywords.Any(kw => title.Contains(kw));
        }

        /// <summary>
        /// Determines if a job is a product role
        /// </summary>
        private bool IsProductRole(string title)
        {
            if (title == null)
                return false;

            var lowerTitle = title.ToLower();
            return lowerTitle.Contains("product manager") || 
                   lowerTitle.Contains("business analyst") ||
                   lowerTitle.Contains("ba");
        }

        /// <summary>
        /// Determines if a job is a design role
        /// </summary>
        private bool IsDesignRole(string title)
        {
            if (title == null)
                return false;

            var lowerTitle = title.ToLower();
            return lowerTitle.Contains("designer") ||
                   lowerTitle.Contains("ui/ux") ||
                   lowerTitle.Contains("ux") ||
                   lowerTitle.Contains("ui");
        }

        /// <summary>
        /// Gets total number of jobs in aggregator
        /// </summary>
        public int GetTotalJobCount() => _allJobs.Count;

        /// <summary>
        /// Clears all jobs from the aggregator
        /// </summary>
        public void Clear()
        {
            _allJobs.Clear();
        }
    }
}
