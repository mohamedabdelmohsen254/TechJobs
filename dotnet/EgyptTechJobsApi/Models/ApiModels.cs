using System;
using System.Collections.Generic;

namespace EgyptTechJobsApi.Models
{
    /// <summary>
    /// Job statistics response
    /// </summary>
    public class JobStatistics
    {
        public int TotalJobs { get; set; }
        public int UniqueCompanies { get; set; }
        public int UniqueCities { get; set; }
        public Dictionary<string, int> SourceBreakdown { get; set; } = new();
        public Dictionary<string, int> LevelBreakdown { get; set; } = new();
        public Dictionary<string, int> WorkTypeBreakdown { get; set; } = new();
    }

    /// <summary>
    /// API Response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Filter options for jobs
    /// </summary>
    public class JobFilterOptions
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public string City { get; set; }
        public string Level { get; set; }
        public string Source { get; set; }
        public string WorkType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
