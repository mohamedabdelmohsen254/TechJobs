using System;
using System.Collections.Generic;

namespace EgyptTechJobs.Models
{
    /// <summary>
    /// Configuration for a job source
    /// </summary>
    public class JobSource
    {
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string SourceType { get; set; } // official_api, rss_feed, scraping, manual
        public string AllowedMode { get; set; } // full_display, limited_display, link_only, disabled
        public bool AttributionRequired { get; set; }
        public RateLimit RateLimit { get; set; }
        public string TakedownContact { get; set; }
        public string SourceUrl { get; set; }
        public string TermsUrl { get; set; }
        public string Notes { get; set; }
    }

    public class RateLimit
    {
        public int RequestsPerMinute { get; set; }
        public int Burst { get; set; }
    }
}
