using System;

namespace EgyptTechJobsApi.Models
{
    /// <summary>
    /// Represents a job listing for API responses
    /// </summary>
    public class JobListing
    {
        public string JobId { get; set; }

        public string Title { get; set; }

        public string Company { get; set; }

        public string Level { get; set; }

        public string Salary { get; set; }

        public string ExperienceYears { get; set; }

        public string Skills { get; set; }

        public string Source { get; set; }

        public string SourceId { get; set; }

        public string SourceType { get; set; }

        public string AllowedMode { get; set; }

        public string AttributionRequired { get; set; }

        public string SourceUrl { get; set; }

        public int RateLimitRpm { get; set; }

        public int RateLimitBurst { get; set; }

        public string TakedownContact { get; set; }

        public string TermsUrl { get; set; }

        public string SourceNotes { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string WorkType { get; set; }

        public string Location { get; set; }

        public string ApplyUrl { get; set; }

        public DateTime? Date { get; set; }
    }
}
