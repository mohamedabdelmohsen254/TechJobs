using System;
using CsvHelper.Configuration.Attributes;

namespace EgyptTechJobs.Models
{
    /// <summary>
    /// Represents a job listing from various sources
    /// </summary>
    public class JobListing
    {
        [Name("Job_ID")]
        public string JobId { get; set; }

        [Name("Title")]
        public string Title { get; set; }

        [Name("Company")]
        public string Company { get; set; }

        [Name("Level")]
        public string Level { get; set; }

        [Name("Salary")]
        public string Salary { get; set; }

        [Name("Experience_Years")]
        public string ExperienceYears { get; set; }

        [Name("Skills")]
        public string Skills { get; set; }

        [Name("Source")]
        public string Source { get; set; }

        [Name("Source_ID")]
        public string SourceId { get; set; }

        [Name("Source_Type")]
        public string SourceType { get; set; }

        [Name("Allowed_Mode")]
        public string AllowedMode { get; set; }

        [Name("Attribution_Required")]
        public string AttributionRequired { get; set; }

        [Name("Source_URL")]
        public string SourceUrl { get; set; }

        [Name("Rate_Limit_RPM")]
        public int RateLimitRpm { get; set; }

        [Name("Rate_Limit_Burst")]
        public int RateLimitBurst { get; set; }

        [Name("Takedown_Contact")]
        public string TakedownContact { get; set; }

        [Name("Terms_URL")]
        public string TermsUrl { get; set; }

        [Name("Source_Notes")]
        public string SourceNotes { get; set; }

        [Name("Country")]
        public string Country { get; set; }

        [Name("City")]
        public string City { get; set; }

        [Name("Work_Type")]
        public string WorkType { get; set; }

        [Name("Location")]
        public string Location { get; set; }

        [Name("Apply_URL")]
        public string ApplyUrl { get; set; }

        [Name("Date")]
        public DateTime Date { get; set; }
    }
}
