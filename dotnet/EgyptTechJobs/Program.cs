using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EgyptTechJobs.Config;
using EgyptTechJobs.Services;

namespace EgyptTechJobs
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  EGYPT TECH JOBS AGGREGATOR — .NET 10 VERSION                      ║");
            Console.WriteLine("║  Aggregates job listings from multiple sources                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // Initialize settings
            var settings = new AppSettings();

            // Initialize services
            var csvService = new CsvService();
            var httpClient = new System.Net.Http.HttpClient 
            { 
                Timeout = TimeSpan.FromSeconds(settings.Timeout) 
            };
            var joobleService = new JoobleApiService(httpClient, settings);
            var aggregatorService = new JobAggregatorService(settings);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Read existing jobs from CSV
                Console.WriteLine("📂 Loading existing job data from CSV...");
                var existingJobs = await csvService.ReadJobsAsync();
                Console.WriteLine($"   ✅ Loaded {existingJobs.Count} existing jobs");
                Console.WriteLine();

                // Add existing jobs to aggregator
                aggregatorService.AddJobs(existingJobs);

                // Fetch jobs from Jooble API
                if (settings.JoobleEnabled)
                {
                    Console.WriteLine("🌐 Fetching jobs from Jooble API...");
                    await FetchJobsFromJooble(joobleService, aggregatorService, settings);
                    Console.WriteLine();
                }

                // Get filtered jobs
                Console.WriteLine("🔍 Filtering jobs based on criteria...");
                var filteredJobs = aggregatorService.GetFilteredJobs();
                Console.WriteLine($"   ✅ Found {filteredJobs.Count} tech jobs in Egypt");
                Console.WriteLine();

                // Save to CSV
                Console.WriteLine("💾 Saving jobs to CSV file...");
                await csvService.WriteJobsAsync(filteredJobs);
                Console.WriteLine("   ✅ Jobs saved successfully");
                Console.WriteLine();

                // Display statistics
                Console.WriteLine("📊 Job Statistics:");
                var stats = csvService.GetStatistics(filteredJobs);
                Console.WriteLine($"   Total Jobs: {stats.TotalJobs}");
                Console.WriteLine($"   Unique Companies: {stats.UniqueCompanies}");
                Console.WriteLine($"   Unique Cities: {stats.UniqueCities}");
                Console.WriteLine();

                Console.WriteLine("Source Breakdown:");
                foreach (var source in stats.SourceBreakdown.OrderByDescending(x => x.Value))
                {
                    Console.WriteLine($"   {source.Key}: {source.Value}");
                }
                Console.WriteLine();

                Console.WriteLine("Level Breakdown:");
                foreach (var level in stats.LevelBreakdown.OrderByDescending(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(level.Key))
                        Console.WriteLine($"   {level.Key}: {level.Value}");
                }
                Console.WriteLine();

                Console.WriteLine("Work Type Breakdown:");
                foreach (var workType in stats.WorkTypeBreakdown.OrderByDescending(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(workType.Key))
                        Console.WriteLine($"   {workType.Key}: {workType.Value}");
                }
                Console.WriteLine();

                stopwatch.Stop();
                Console.WriteLine($"⏱️  Total execution time: {stopwatch.Elapsed.TotalSeconds:F2} seconds");
                Console.WriteLine();
                Console.WriteLine("✅ Process completed successfully!");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
            finally
            {
                httpClient?.Dispose();
            }
        }

        static async Task FetchJobsFromJooble(
            JoobleApiService joobleService, 
            JobAggregatorService aggregatorService,
            AppSettings settings)
        {
            int keywordCount = 0;
            int totalJobsFetched = 0;

            foreach (var keyword in settings.JoobleSearchKeywords)
            {
                try
                {
                    var jobs = await joobleService.FetchJobsByKeywordAsync(keyword);
                    aggregatorService.AddJobs(jobs);
                    totalJobsFetched += jobs.Count;
                    keywordCount++;

                    if (jobs.Count > 0)
                    {
                        Console.WriteLine($"   ✓ '{keyword}': {jobs.Count} jobs");
                    }

                    // Rate limiting
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ✗ Error fetching jobs for '{keyword}': {ex.Message}");
                }
            }

            Console.WriteLine($"   ✅ Fetched {totalJobsFetched} jobs from {keywordCount} search keywords");
        }
    }
}

