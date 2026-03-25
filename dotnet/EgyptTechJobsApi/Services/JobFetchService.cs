using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EgyptTechJobsApi.Application.Abstractions;
using EgyptTechJobsApi.Data;
using EgyptTechJobsApi.Models;
using EgyptTechJobsApi.Models.Entities;

namespace EgyptTechJobsApi.Services
{
    /// <summary>
    /// Service for fetching jobs from multiple sources and saving to database
    /// </summary>
    public class JobFetchService : IJobFetchService
    {
        private readonly HttpClient _httpClient;
        private readonly JobsDbContext _dbContext;
        private readonly ILogger<JobFetchService> _logger;
        private readonly string _configPath;

        // Configuration
        private const int TIMEOUT_SECONDS = 20;
        private const string JOOBLE_API_KEY = "08cfec52-7791-487b-9c40-a3e45efe9aa3";

        // Company boards
        private static readonly string[] GREENHOUSE_BOARDS = {
            "careem", "propertyfinder", "swvl", "paymob", "vezeeta", "instabug",
            "canonical", "mongodb", "elastic", "gitlab", "twilio", "datadog",
            "stripe", "figma", "airtable", "asana", "dropbox", "intercom",
            "mixpanel", "braze", "calendly", "typeform", "webflow", "vercel",
            "planetscale", "cockroachlabs", "launchdarkly", "contentful", "algolia",
            "cloudflare", "fastly", "okta", "pagerduty", "speechify", "udacity",
            "coursera", "duolingo", "agoda", "trivago", "n26", "monzo", "marqeta",
            "gusto", "remote", "lattice", "salesloft", "apollo", "deel", "oyster"
        };

        private static readonly string[] LEVER_SITES = {
            "Bosta", "Yassir", "soum", "econstruct",
            "welocalize", "rws", "aleph", "gradion", "toptal", "neon",
            "metabase", "zerotier", "teleport", "secureframe", "sysdig"
        };

        private static readonly string[] WORKABLE_SLUGS = {
            "cequens", "integrant", "blabs", "blink22-3", "nawy-real-estate",
            "robusta", "rubikal", "sumerge-1", "finaira", "egyptian-banks-company-4",
            "bm-to", "money-fellows", "advansys-esc-1", "mylo-btech", "infomineo",
            "xenon7", "dsquares-loyalty-dmcc", "tagaddod", "dopay-8", "adree",
            "nowlun", "lawazem", "flat6labs", "covergo", "foodics", "gathern"
        };

        private static readonly string[] JOOBLE_KEYWORDS = {
            "oracle developer", "sql developer", "database developer", "plsql developer",
            "software engineer", "backend developer", "frontend developer", "fullstack developer",
            "python developer", "java developer", "data engineer", "devops engineer",
            "mobile developer", "react developer", "angular developer",
            "remote software", "remote developer", "remote engineer"
        };

        public JobFetchService(HttpClient httpClient, JobsDbContext dbContext, ILogger<JobFetchService> logger, IWebHostEnvironment environment)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _dbContext = dbContext;
            _logger = logger;

            var configPath = Path.Combine(environment.ContentRootPath, "..", "..", "data", "job_sources_config.json");
            _configPath = Path.GetFullPath(configPath);
        }

        /// <summary>
        /// Fetch jobs from all enabled sources and save to CSV
        /// </summary>
        public async Task<FetchResult> FetchAndSaveJobsAsync(FetchOptions options)
        {
            var result = new FetchResult { StartTime = DateTime.UtcNow };
            var allJobs = new List<JobListing>();
            var sourceStats = new Dictionary<string, int>();

            try
            {
                // Fetch from enabled sources in parallel
                var tasks = new List<Task<List<JobListing>>>();

                if (options.FetchGreenhouse)
                    tasks.Add(FetchGreenhouseJobsAsync());
                if (options.FetchLever)
                    tasks.Add(FetchLeverJobsAsync());
                if (options.FetchWorkable)
                    tasks.Add(FetchWorkableJobsAsync());
                if (options.FetchJooble)
                    tasks.Add(FetchJoobleJobsAsync(options.JoobleMaxPages));
                if (options.FetchRemoteOk)
                    tasks.Add(FetchRemoteOkJobsAsync());
                if (options.FetchRemotive)
                    tasks.Add(FetchRemotiveJobsAsync());
                if (options.FetchHimalayas)
                    tasks.Add(FetchHimalayasJobsAsync());
                if (options.FetchJobicy)
                    tasks.Add(FetchJobicyJobsAsync());

                var results = await Task.WhenAll(tasks);

                foreach (var jobs in results)
                {
                    if (jobs != null && jobs.Count > 0)
                    {
                        allJobs.AddRange(jobs);
                        var source = jobs.First().Source;
                        sourceStats[source] = jobs.Count;
                    }
                }

                // Deduplicate jobs
                var dedupedJobs = DeduplicateJobs(allJobs);
                result.TotalFetched = allJobs.Count;
                result.AfterDedup = dedupedJobs.Count;

                // Save to database
                var savedCount = await SaveJobsToDbAsync(dedupedJobs);
                result.SavedToDb = savedCount;
                result.Success = true;
                result.SourceStats = sourceStats;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                _logger.LogError(ex, "Error fetching and saving jobs");
            }

            result.EndTime = DateTime.UtcNow;
            return result;
        }

        #region Greenhouse Fetcher

        private async Task<List<JobListing>> FetchGreenhouseJobsAsync()
        {
            var allJobs = new List<JobListing>();

            var tasks = GREENHOUSE_BOARDS.Select(async board =>
            {
                try
                {
                    var url = $"https://boards-api.greenhouse.io/v1/boards/{board}/jobs";
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode) return new List<JobListing>();

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonDocument.Parse(json);
                    var jobs = new List<JobListing>();

                    if (data.RootElement.TryGetProperty("jobs", out var jobsArray))
                    {
                        foreach (var job in jobsArray.EnumerateArray())
                        {
                            var location = "";
                            if (job.TryGetProperty("location", out var locProp))
                            {
                                if (locProp.ValueKind == JsonValueKind.Object)
                                    location = locProp.GetProperty("name").GetString() ?? "";
                                else if (locProp.ValueKind == JsonValueKind.String)
                                    location = locProp.GetString() ?? "";
                            }

                            var title = job.GetProperty("title").GetString() ?? "";
                            var applyUrl = job.TryGetProperty("absolute_url", out var urlProp) 
                                ? urlProp.GetString() ?? "" : "";

                            DateTime? date = null;
                            if (job.TryGetProperty("updated_at", out var dateProp) &&
                                DateTime.TryParse(dateProp.GetString(), out var d))
                                date = d;

                            jobs.Add(CreateJobListing(
                                title: title,
                                company: FormatCompanyName(board),
                                location: location,
                                applyUrl: applyUrl,
                                date: date,
                                source: "Greenhouse"
                            ));
                        }
                    }
                    return jobs;
                }
                catch { return new List<JobListing>(); }
            });

            var results = await Task.WhenAll(tasks);
            foreach (var jobs in results)
                allJobs.AddRange(jobs);

            return allJobs;
        }

        #endregion

        #region Lever Fetcher

        private async Task<List<JobListing>> FetchLeverJobsAsync()
        {
            var allJobs = new List<JobListing>();

            var tasks = LEVER_SITES.Select(async site =>
            {
                try
                {
                    var url = $"https://api.lever.co/v0/postings/{site}?mode=json";
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode) return new List<JobListing>();

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonDocument.Parse(json);
                    var jobs = new List<JobListing>();

                    if (data.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var job in data.RootElement.EnumerateArray())
                        {
                            var title = job.TryGetProperty("text", out var t) ? t.GetString() ?? "" : "";
                            var location = "";
                            if (job.TryGetProperty("categories", out var cats) && 
                                cats.TryGetProperty("location", out var locProp))
                                location = locProp.GetString() ?? "";

                            var applyUrl = job.TryGetProperty("hostedUrl", out var urlProp) 
                                ? urlProp.GetString() ?? "" : "";

                            DateTime? date = null;
                            if (job.TryGetProperty("createdAt", out var dateProp) && 
                                dateProp.ValueKind == JsonValueKind.Number)
                            {
                                var timestamp = dateProp.GetInt64();
                                date = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
                            }

                            jobs.Add(CreateJobListing(
                                title: title,
                                company: FormatCompanyName(site),
                                location: location,
                                applyUrl: applyUrl,
                                date: date,
                                source: "Lever"
                            ));
                        }
                    }
                    return jobs;
                }
                catch { return new List<JobListing>(); }
            });

            var results = await Task.WhenAll(tasks);
            foreach (var jobs in results)
                allJobs.AddRange(jobs);

            return allJobs;
        }

        #endregion

        #region Workable Fetcher

        private async Task<List<JobListing>> FetchWorkableJobsAsync()
        {
            var allJobs = new List<JobListing>();

            var tasks = WORKABLE_SLUGS.Select(async slug =>
            {
                try
                {
                    var url = $"https://apply.workable.com/api/v1/widget/accounts/{slug}";
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode) return new List<JobListing>();

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonDocument.Parse(json);
                    var jobs = new List<JobListing>();

                    var companyName = FormatCompanyName(slug);
                    if (data.RootElement.TryGetProperty("account", out var account) &&
                        account.TryGetProperty("name", out var nameProp))
                        companyName = nameProp.GetString() ?? companyName;

                    var jobsArray = data.RootElement.TryGetProperty("jobs", out var jobsProp) 
                        ? jobsProp : (data.RootElement.TryGetProperty("results", out var resProp) 
                        ? resProp : default);

                    if (jobsArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var job in jobsArray.EnumerateArray())
                        {
                            var title = job.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                            var location = "";
                            if (job.TryGetProperty("location", out var locProp))
                            {
                                if (locProp.ValueKind == JsonValueKind.Object)
                                    location = locProp.TryGetProperty("city", out var c) ? c.GetString() ?? "" : "";
                                else if (locProp.ValueKind == JsonValueKind.String)
                                    location = locProp.GetString() ?? "";
                            }

                            var applyUrl = job.TryGetProperty("url", out var urlProp) 
                                ? urlProp.GetString() ?? "" : "";

                            DateTime? date = null;
                            if (job.TryGetProperty("published", out var dateProp) &&
                                DateTime.TryParse(dateProp.GetString(), out var d))
                                date = d;

                            jobs.Add(CreateJobListing(
                                title: title,
                                company: companyName,
                                location: location,
                                applyUrl: applyUrl,
                                date: date,
                                source: "Workable"
                            ));
                        }
                    }
                    return jobs;
                }
                catch { return new List<JobListing>(); }
            });

            var results = await Task.WhenAll(tasks);
            foreach (var jobs in results)
                allJobs.AddRange(jobs);

            return allJobs;
        }

        #endregion

        #region Jooble Fetcher

        private async Task<List<JobListing>> FetchJoobleJobsAsync(int maxPages = 3)
        {
            var allJobs = new List<JobListing>();

            foreach (var keyword in JOOBLE_KEYWORDS)
            {
                for (int page = 1; page <= maxPages; page++)
                {
                    try
                    {
                        var url = $"https://jooble.org/api/{JOOBLE_API_KEY}";
                        var payload = JsonSerializer.Serialize(new { keywords = keyword, location = "Egypt", page = page });
                        var content = new StringContent(payload, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync(url, content);
                        if (!response.IsSuccessStatusCode) break;

                        var json = await response.Content.ReadAsStringAsync();
                        var data = JsonDocument.Parse(json);

                        if (!data.RootElement.TryGetProperty("jobs", out var jobsArray))
                            break;

                        var jobCount = 0;
                        foreach (var job in jobsArray.EnumerateArray())
                        {
                            jobCount++;
                            var title = job.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                            var company = job.TryGetProperty("company", out var c) ? c.GetString() ?? "Unknown" : "Unknown";
                            var location = job.TryGetProperty("location", out var l) ? l.GetString() ?? "" : "";
                            var link = job.TryGetProperty("link", out var lnk) ? lnk.GetString() ?? "" : "";

                            DateTime? date = null;
                            if (job.TryGetProperty("updated", out var dateProp) &&
                                DateTime.TryParse(dateProp.GetString(), out var d))
                                date = d;

                            // Clean company name
                            company = Regex.Replace(company, @"\s*[-–|•]\s*.*$", "");
                            if (company.Length > 50) company = company.Substring(0, 50);

                            allJobs.Add(CreateJobListing(
                                title: title,
                                company: company,
                                location: location,
                                applyUrl: link,
                                date: date,
                                source: "Jooble"
                            ));
                        }

                        if (jobCount < 20) break; // No more pages
                    }
                    catch { break; }
                }
            }

            return allJobs;
        }

        #endregion

        #region RemoteOK Fetcher

        private async Task<List<JobListing>> FetchRemoteOkJobsAsync()
        {
            var jobs = new List<JobListing>();

            try
            {
                var response = await _httpClient.GetAsync("https://remoteok.com/api");
                if (!response.IsSuccessStatusCode) return jobs;

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(json);

                if (data.RootElement.ValueKind != JsonValueKind.Array) return jobs;

                var isFirst = true;
                foreach (var job in data.RootElement.EnumerateArray())
                {
                    // Skip metadata (first item)
                    if (isFirst) { isFirst = false; continue; }

                    var title = job.TryGetProperty("position", out var t) ? t.GetString() ?? "" : "";
                    var company = job.TryGetProperty("company", out var c) ? c.GetString() ?? "Unknown" : "Unknown";
                    var location = job.TryGetProperty("location", out var l) ? l.GetString() ?? "Remote (Worldwide)" : "Remote (Worldwide)";
                    var applyUrl = job.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";

                    DateTime? date = null;
                    if (job.TryGetProperty("date", out var dateProp) &&
                        DateTime.TryParse(dateProp.GetString(), out var d))
                        date = d;

                    if (!location.ToLower().Contains("remote"))
                        location = "Remote - " + location;

                    jobs.Add(CreateJobListing(
                        title: title,
                        company: company,
                        location: location,
                        applyUrl: applyUrl,
                        date: date,
                        source: "RemoteOK"
                    ));
                }
            }
            catch { }

            return jobs;
        }

        #endregion

        #region Remotive Fetcher

        private async Task<List<JobListing>> FetchRemotiveJobsAsync()
        {
            var jobs = new List<JobListing>();
            var categories = new[] { "software-dev", "data", "devops-sysadmin", "product", "qa" };

            foreach (var category in categories)
            {
                try
                {
                    var url = $"https://remotive.com/api/remote-jobs?category={category}";
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonDocument.Parse(json);

                    if (!data.RootElement.TryGetProperty("jobs", out var jobsArray)) continue;

                    foreach (var job in jobsArray.EnumerateArray())
                    {
                        var title = job.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                        var company = job.TryGetProperty("company_name", out var c) ? c.GetString() ?? "Unknown" : "Unknown";
                        var location = job.TryGetProperty("candidate_required_location", out var l) 
                            ? l.GetString() ?? "Remote (Worldwide)" : "Remote (Worldwide)";
                        var applyUrl = job.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";

                        DateTime? date = null;
                        if (job.TryGetProperty("publication_date", out var dateProp) &&
                            DateTime.TryParse(dateProp.GetString(), out var d))
                            date = d;

                        if (!location.ToLower().Contains("remote"))
                            location = "Remote - " + location;

                        jobs.Add(CreateJobListing(
                            title: title,
                            company: company,
                            location: location,
                            applyUrl: applyUrl,
                            date: date,
                            source: "Remotive"
                        ));
                    }
                }
                catch { }
            }

            return jobs;
        }

        #endregion

        #region Himalayas Fetcher

        private async Task<List<JobListing>> FetchHimalayasJobsAsync()
        {
            var jobs = new List<JobListing>();

            try
            {
                var response = await _httpClient.GetAsync("https://himalayas.app/jobs/api?limit=100");
                if (!response.IsSuccessStatusCode) return jobs;

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(json);

                if (!data.RootElement.TryGetProperty("jobs", out var jobsArray)) return jobs;

                foreach (var job in jobsArray.EnumerateArray())
                {
                    var title = job.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                    var company = job.TryGetProperty("companyName", out var c) ? c.GetString() ?? "Unknown" : "Unknown";
                    var location = job.TryGetProperty("location", out var l) ? l.GetString() ?? "Remote (Worldwide)" : "Remote (Worldwide)";
                    var applyUrl = job.TryGetProperty("applicationUrl", out var u) ? u.GetString() ?? "" : "";

                    DateTime? date = null;
                    if (job.TryGetProperty("pubDate", out var dateProp) &&
                        DateTime.TryParse(dateProp.GetString(), out var d))
                        date = d;

                    if (!location.ToLower().Contains("remote"))
                        location = "Remote - " + location;

                    jobs.Add(CreateJobListing(
                        title: title,
                        company: company,
                        location: location,
                        applyUrl: applyUrl,
                        date: date,
                        source: "Himalayas"
                    ));
                }
            }
            catch { }

            return jobs;
        }

        #endregion

        #region Jobicy Fetcher

        private async Task<List<JobListing>> FetchJobicyJobsAsync()
        {
            var jobs = new List<JobListing>();

            try
            {
                var response = await _httpClient.GetAsync("https://jobicy.com/api/v2/remote-jobs?count=50&industry=dev");
                if (!response.IsSuccessStatusCode) return jobs;

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonDocument.Parse(json);

                if (!data.RootElement.TryGetProperty("jobs", out var jobsArray)) return jobs;

                foreach (var job in jobsArray.EnumerateArray())
                {
                    var title = job.TryGetProperty("jobTitle", out var t) ? t.GetString() ?? "" : "";
                    var company = job.TryGetProperty("companyName", out var c) ? c.GetString() ?? "Unknown" : "Unknown";
                    var geo = job.TryGetProperty("jobGeo", out var g) ? g.GetString() ?? "Worldwide" : "Worldwide";
                    var location = $"Remote - {geo}";
                    var applyUrl = job.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";

                    DateTime? date = null;
                    if (job.TryGetProperty("pubDate", out var dateProp) &&
                        DateTime.TryParse(dateProp.GetString(), out var d))
                        date = d;

                    jobs.Add(CreateJobListing(
                        title: title,
                        company: company,
                        location: location,
                        applyUrl: applyUrl,
                        date: date,
                        source: "Jobicy"
                    ));
                }
            }
            catch { }

            return jobs;
        }

        #endregion

        #region Helper Methods

        private JobListing CreateJobListing(string title, string company, string location, 
            string applyUrl, DateTime? date, string source)
        {
            var country = DetectCountry(location, company);
            var city = DetectEgyptCity(location);
            var workType = DetectWorkType(location, title, country);
            var level = DetectLevel(title);
            var skills = ExtractSkills(title);

            // Load source config
            var sourceConfig = LoadSourceConfig(source);

            return new JobListing
            {
                JobId = GenerateJobId(title, company, applyUrl),
                Title = title,
                Company = company,
                Level = level,
                Salary = "",
                ExperienceYears = "",
                Skills = skills,
                Source = source,
                SourceId = sourceConfig.SourceId,
                SourceType = sourceConfig.SourceType,
                AllowedMode = sourceConfig.AllowedMode,
                AttributionRequired = sourceConfig.AttributionRequired,
                SourceUrl = sourceConfig.SourceUrl,
                RateLimitRpm = sourceConfig.RateLimitRpm,
                RateLimitBurst = sourceConfig.RateLimitBurst,
                TakedownContact = sourceConfig.TakedownContact,
                TermsUrl = sourceConfig.TermsUrl,
                SourceNotes = sourceConfig.SourceNotes,
                Country = country,
                City = city,
                WorkType = workType,
                Location = location,
                ApplyUrl = applyUrl,
                Date = date
            };
        }

        private string GenerateJobId(string title, string company, string url)
        {
            var hash = $"{title}{company}{url}".GetHashCode();
            return hash.ToString();
        }

        private string FormatCompanyName(string slug)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                slug.Replace("-", " ").Replace("_", " "));
        }

        private string DetectCountry(string location, string company)
        {
            if (string.IsNullOrEmpty(location)) return "Unknown";
            var loc = location.ToLower();

            var egyptKeywords = new[] { "egypt", "cairo", "giza", "alexandria", "new cairo", "tagamoa", "maadi" };
            if (egyptKeywords.Any(k => loc.Contains(k))) return "Egypt";

            if (loc.Contains("remote") || loc.Contains("worldwide")) return "Remote";
            if (loc.Contains("uae") || loc.Contains("dubai")) return "UAE";
            if (loc.Contains("saudi") || loc.Contains("riyadh")) return "Saudi Arabia";
            if (loc.Contains("usa") || loc.Contains("united states")) return "USA";
            if (loc.Contains("uk") || loc.Contains("london")) return "UK";
            if (loc.Contains("canada")) return "Canada";

            return "Other";
        }

        private string DetectEgyptCity(string location)
        {
            if (string.IsNullOrEmpty(location)) return "";
            var loc = location.ToLower();

            if (loc.Contains("new cairo") || loc.Contains("tagamoa") || loc.Contains("fifth settlement"))
                return "New Cairo";
            if (loc.Contains("cairo")) return "Cairo";
            if (loc.Contains("giza") || loc.Contains("6th of october") || loc.Contains("sheikh zayed"))
                return "Giza";
            if (loc.Contains("alexandria") || loc.Contains("alex")) return "Alexandria";
            if (loc.Contains("smart village")) return "Smart Village";
            if (loc.Contains("egypt")) return "Egypt (General)";

            return "";
        }

        private string DetectWorkType(string location, string title, string country)
        {
            var text = $"{location} {title}".ToLower();

            if (text.Contains("hybrid")) return "Hybrid";
            if (text.Contains("remote") || text.Contains("work from home") || text.Contains("worldwide"))
                return "Remote";

            return country == "Egypt" ? "On-site" : "Relocation Required";
        }

        private string DetectLevel(string title)
        {
            var t = title.ToLower();

            if (new[] { "principal", "staff engineer", "distinguished" }.Any(k => t.Contains(k)))
                return "Principal";
            if (new[] { "lead", "team lead", "tech lead", "head of", "director", "manager" }.Any(k => t.Contains(k)))
                return "Lead";
            if (new[] { "senior", "sr.", "sr " }.Any(k => t.Contains(k)))
                return "Senior";
            if (new[] { "junior", "jr.", "jr ", "entry", "graduate", "trainee" }.Any(k => t.Contains(k)))
                return "Junior";
            if (new[] { "intern", "internship" }.Any(k => t.Contains(k)))
                return "Intern";

            return "Mid";
        }

        private string ExtractSkills(string title)
        {
            var skills = new List<string>();
            var t = title.ToLower();

            var skillMap = new Dictionary<string, string>
            {
                { "python", "Python" }, { "java", "Java" }, { "javascript", "JavaScript" },
                { "typescript", "TypeScript" }, { "react", "React" }, { "angular", "Angular" },
                { "vue", "Vue.js" }, { "node", "Node.js" }, { ".net", "C#/.NET" },
                { "c#", "C#/.NET" }, { "golang", "Go" }, { "rust", "Rust" },
                { "aws", "AWS" }, { "azure", "Azure" }, { "gcp", "GCP" },
                { "kubernetes", "Kubernetes" }, { "docker", "Docker" }, { "devops", "DevOps" },
                { "sql", "SQL" }, { "oracle", "Oracle" }, { "mongodb", "MongoDB" },
                { "postgresql", "PostgreSQL" }, { "mysql", "MySQL" },
                { "machine learning", "Machine Learning" }, { "data science", "Data Science" },
                { "android", "Android" }, { "ios", "iOS" }, { "flutter", "Flutter" },
                { "backend", "Backend" }, { "frontend", "Frontend" }, { "full stack", "Full Stack" }
            };

            foreach (var kvp in skillMap)
            {
                if (t.Contains(kvp.Key) && !skills.Contains(kvp.Value))
                    skills.Add(kvp.Value);
            }

            return string.Join(", ", skills.Take(5));
        }

        private SourceConfig LoadSourceConfig(string source)
        {
            var defaults = new Dictionary<string, SourceConfig>
            {
                ["Greenhouse"] = new SourceConfig { SourceId = "greenhouse", SourceType = "official_api", AllowedMode = "full_display", AttributionRequired = "No", SourceUrl = "https://boards.greenhouse.io", RateLimitRpm = 60, RateLimitBurst = 10, TakedownContact = "support@greenhouse.io", TermsUrl = "https://www.greenhouse.io/terms-of-service", SourceNotes = "Public job board API." },
                ["Lever"] = new SourceConfig { SourceId = "lever", SourceType = "official_api", AllowedMode = "full_display", AttributionRequired = "No", SourceUrl = "https://jobs.lever.co", RateLimitRpm = 60, RateLimitBurst = 10, TakedownContact = "privacy@lever.co", TermsUrl = "https://www.lever.co/terms-of-service", SourceNotes = "Public job board API." },
                ["Workable"] = new SourceConfig { SourceId = "workable", SourceType = "official_api", AllowedMode = "full_display", AttributionRequired = "No", SourceUrl = "https://apply.workable.com", RateLimitRpm = 60, RateLimitBurst = 10, TakedownContact = "privacy@workable.com", TermsUrl = "https://www.workable.com/terms", SourceNotes = "Public job board API." },
                ["Jooble"] = new SourceConfig { SourceId = "jooble", SourceType = "official_api", AllowedMode = "full_display", AttributionRequired = "No", SourceUrl = "https://jooble.org", RateLimitRpm = 30, RateLimitBurst = 5, TakedownContact = "support@jooble.org", TermsUrl = "https://jooble.org/info/terms-of-use", SourceNotes = "Partner API." },
                ["RemoteOK"] = new SourceConfig { SourceId = "remoteok", SourceType = "official_api", AllowedMode = "limited_display", AttributionRequired = "Yes", SourceUrl = "https://remoteok.com", RateLimitRpm = 10, RateLimitBurst = 2, TakedownContact = "pieter@levels.io", TermsUrl = "https://remoteok.com/legal", SourceNotes = "Public API. Attribution required." },
                ["Remotive"] = new SourceConfig { SourceId = "remotive", SourceType = "official_api", AllowedMode = "limited_display", AttributionRequired = "Yes", SourceUrl = "https://remotive.io", RateLimitRpm = 30, RateLimitBurst = 5, TakedownContact = "hello@remotive.io", TermsUrl = "https://remotive.io/terms", SourceNotes = "Public API. Attribution required." },
                ["Himalayas"] = new SourceConfig { SourceId = "himalayas", SourceType = "official_api", AllowedMode = "link_only", AttributionRequired = "Yes", SourceUrl = "https://himalayas.app", RateLimitRpm = 20, RateLimitBurst = 3, TakedownContact = "hello@himalayas.app", TermsUrl = "https://himalayas.app/terms", SourceNotes = "Terms restrict display." },
                ["Jobicy"] = new SourceConfig { SourceId = "jobicy", SourceType = "official_api", AllowedMode = "limited_display", AttributionRequired = "Yes", SourceUrl = "https://jobicy.com", RateLimitRpm = 20, RateLimitBurst = 3, TakedownContact = "hello@jobicy.com", TermsUrl = "https://jobicy.com/terms", SourceNotes = "Public API with RSS." }
            };

            return defaults.TryGetValue(source, out var config) ? config : new SourceConfig
            {
                SourceId = source.ToLower(),
                SourceType = "manual",
                AllowedMode = "link_only",
                AttributionRequired = "Yes",
                SourceUrl = "",
                RateLimitRpm = 5,
                RateLimitBurst = 1,
                TakedownContact = "",
                TermsUrl = "",
                SourceNotes = "Unknown source."
            };
        }

        private List<JobListing> DeduplicateJobs(List<JobListing> jobs)
        {
            var seen = new HashSet<string>();
            var result = new List<JobListing>();

            foreach (var job in jobs)
            {
                var key = $"{job.Company?.ToLower()}|{job.Title?.ToLower()}|{NormalizeUrl(job.ApplyUrl)}";
                if (!seen.Contains(key))
                {
                    seen.Add(key);
                    result.Add(job);
                }
            }

            return result;
        }

        private string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            try
            {
                var uri = new Uri(url.ToLower().Trim());
                return $"{uri.Host}{uri.AbsolutePath.TrimEnd('/')}";
            }
            catch { return url.ToLower().Trim(); }
        }

        private async Task<int> SaveJobsToDbAsync(List<JobListing> jobs)
        {
            var savedCount = 0;
            
            foreach (var jobListing in jobs)
            {
                try
                {
                    // Check if job already exists by JobId
                    var existingJob = await _dbContext.Jobs
                        .FirstOrDefaultAsync(j => j.JobId == jobListing.JobId.Truncate(200));

                    if (existingJob != null)
                    {
                        // Update existing job
                        existingJob.Title = jobListing.Title.Truncate(500);
                        existingJob.Company = jobListing.Company.Truncate(200);
                        existingJob.SalaryRange = jobListing.Salary.Truncate(500);
                        existingJob.Source = jobListing.Source.Truncate(100);
                        existingJob.Country = jobListing.Country.Truncate(100);
                        existingJob.City = jobListing.City.Truncate(100);
                        existingJob.WorkType = jobListing.WorkType.Truncate(50);
                        existingJob.Location = jobListing.Location.Truncate(200);
                        existingJob.ApplyUrl = jobListing.ApplyUrl.Truncate(1000);
                        existingJob.PostedDate = ToUtc(jobListing.Date);
                        existingJob.UpdatedAt = DateTime.UtcNow;
                        // Store extra metadata in Tags
                        existingJob.Tags = BuildTags(jobListing);
                    }
                    else
                    {
                        // Create new job
                        var job = new Job
                        {
                            JobId = jobListing.JobId.Truncate(200),
                            Title = jobListing.Title.Truncate(500),
                            Company = jobListing.Company.Truncate(200),
                            SalaryRange = jobListing.Salary.Truncate(500),
                            Source = jobListing.Source.Truncate(100),
                            Country = jobListing.Country.Truncate(100),
                            City = jobListing.City.Truncate(100),
                            WorkType = jobListing.WorkType.Truncate(50),
                            Location = jobListing.Location.Truncate(200),
                            ApplyUrl = (jobListing.ApplyUrl ?? "").Truncate(1000),
                            PostedDate = ToUtc(jobListing.Date),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            IsActive = true,
                            IsVisibleToUsers = true,
                            IsManualEntry = false,
                            Tags = BuildTags(jobListing)
                        };
                        await _dbContext.Jobs.AddAsync(job);
                        savedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to save job {JobId}", jobListing.JobId);
                }
            }

            await _dbContext.SaveChangesAsync();
            return savedCount;
        }

        /// <summary>
        /// Convert DateTime to UTC (PostgreSQL requires UTC for timestamptz columns)
        /// </summary>
        private static DateTime? ToUtc(DateTime? dateTime)
        {
            if (dateTime == null) return null;
            var dt = dateTime.Value;
            if (dt.Kind == DateTimeKind.Utc) return dt;
            if (dt.Kind == DateTimeKind.Local) return dt.ToUniversalTime();
            // For Unspecified, assume it's already UTC
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }

        private static string BuildTags(JobListing job)
        {
            var tags = new List<string>();
            if (!string.IsNullOrEmpty(job.Skills)) tags.Add(job.Skills);
            if (!string.IsNullOrEmpty(job.Level)) tags.Add($"Level:{job.Level}");
            if (!string.IsNullOrEmpty(job.ExperienceYears)) tags.Add($"Exp:{job.ExperienceYears}");
            return string.Join(",", tags).Truncate(500);
        }

        #endregion
    }

    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    #region Models

    public class FetchOptions
    {
        public bool FetchGreenhouse { get; set; } = true;
        public bool FetchLever { get; set; } = true;
        public bool FetchWorkable { get; set; } = true;
        public bool FetchJooble { get; set; } = true;
        public bool FetchRemoteOk { get; set; } = true;
        public bool FetchRemotive { get; set; } = true;
        public bool FetchHimalayas { get; set; } = true;
        public bool FetchJobicy { get; set; } = true;
        public int JoobleMaxPages { get; set; } = 3;
    }

    public class FetchResult
    {
        public bool Success { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalFetched { get; set; }
        public int AfterDedup { get; set; }
        public int SavedToDb { get; set; }
        public string Error { get; set; }
        public Dictionary<string, int> SourceStats { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }

    public class SourceConfig
    {
        public string SourceId { get; set; } = "";
        public string SourceType { get; set; } = "";
        public string AllowedMode { get; set; } = "";
        public string AttributionRequired { get; set; } = "";
        public string SourceUrl { get; set; } = "";
        public int RateLimitRpm { get; set; }
        public int RateLimitBurst { get; set; }
        public string TakedownContact { get; set; } = "";
        public string TermsUrl { get; set; } = "";
        public string SourceNotes { get; set; } = "";
    }

    #endregion
}
