# Egypt Tech Jobs Aggregator - .NET 10 Version

A high-performance job aggregation application built with .NET 10 that collects and filters technology job listings from Egypt across multiple sources.

## Overview

This project is a port of the original Python-based Egypt Tech Jobs Aggregator to .NET 10, providing:

- **Multiple Data Sources**: Integrates with Jooble API and supports other job sources
- **Advanced Filtering**: Filters jobs by location, industry, role type, and more
- **CSV Export**: Outputs aggregated jobs to CSV format for easy analysis
- **Statistics**: Provides detailed breakdowns by source, level, and work type
- **High Performance**: Concurrent processing with configurable worker threads
- **Async/Await**: Fully asynchronous architecture for improved responsiveness

## Project Structure

```
EgyptTechJobs/
├── Models/
│   ├── JobListing.cs          # Core job data model
│   └── JobSource.cs           # Job source configuration model
├── Services/
│   ├── CsvService.cs          # CSV read/write operations
│   ├── JoobleApiService.cs    # Jooble API integration
│   └── JobAggregatorService.cs # Job filtering and aggregation
├── Config/
│   └── AppSettings.cs         # Application configuration
└── Program.cs                 # Main entry point
```

## Features

### Job Filtering
- **Egypt Only**: Filters to show only Egypt-based positions
- **Tech Only**: Shows only technology-related roles
- **Remote Support**: Includes remote positions if they mention Egypt
- **Role Exclusion**: Options to exclude Product Manager and Design roles

### Data Sources
- **Jooble API**: Official API integration for job listings
- **CSV Import**: Load existing job data from CSV files

### Export & Analysis
- **CSV Export**: Standardized CSV output with comprehensive fields
- **Statistics**: Detailed job counts by:
  - Experience Level (Entry, Mid, Senior, etc.)
  - Work Type (Remote, On-site, Hybrid)
  - Location (Cairo, Alexandria, etc.)

## Configuration

Edit `AppSettings.cs` to customize:

```csharp
var settings = new AppSettings
{
    EgyptOnly = true,              // Filter Egypt only
    TechOnly = true,               // Filter tech jobs only
    IncludeRemoteEgypt = true,    // Include remote jobs
    IncludeProduct = true,         // Include product/BA roles
    IncludeDesign = false,         // Exclude design roles
    Timeout = 20,                  // HTTP timeout in seconds
    MaxWorkers = 30,               // Concurrent threads
    JoobleEnabled = true,          // Enable Jooble API
    JoobleDaysBack = 14,           // Jobs from last N days
    JoobleMaxPages = 5             // Max pages per search
};
```

### Search Keywords

Customize the `JoobleSearchKeywords` list in `AppSettings.cs` to search for:
- Specific technologies (Python, Java, C#, etc.)
- Specific roles (Developer, Engineer, QA, DBA, etc.)
- Skills (SQL, Cloud, DevOps, etc.)

## Building and Running

### Prerequisites
- .NET 10 SDK
- No external dependencies besides CsvHelper (auto-installed via NuGet)

### Build
```bash
cd EgyptTechJobs
dotnet build
```

### Run
```bash
dotnet run
```

### Publish
```bash
dotnet publish -c Release -o ./publish
```

## Output

The application generates:

1. **Egypt_Tech_Jobs.csv** - CSV file with all aggregated jobs containing:
   - Job ID, Title, Company, Level
   - Salary, Experience, Skills
   - Source information and URLs
   - Location and work type
   - Application link

2. **Console Output** - Real-time progress and statistics:
   ```
   ╔════════════════════════════════════════════════════════════════════╗
   ║  EGYPT TECH JOBS AGGREGATOR — .NET 10 VERSION                      ║
   ║  Aggregates job listings from multiple sources                    ║
   ╚════════════════════════════════════════════════════════════════════╝

   📂 Loading existing job data from CSV...
   ✅ Loaded 1555 existing jobs

   🌐 Fetching jobs from Jooble API...
   ✓ 'software engineer': 45 jobs
   ✓ 'backend developer': 32 jobs
   ...

   📊 Job Statistics:
      Total Jobs: 2847
      Unique Companies: 456
      Unique Cities: 8
   ```

## API Keys

The application uses the Jooble API. The default API key is included in `AppSettings.cs`:
```csharp
JoobleApiKey = Environment.GetEnvironmentVariable("JOOBLE_API_KEY") ?? string.Empty
```

For production use, consider:
- Moving the API key to environment variables
- Using Azure Key Vault for secure storage
- Implementing rate limiting based on API tier

## Performance Characteristics

- **Concurrent Processing**: Uses up to 30 worker threads (configurable)
- **Memory Efficient**: Processes jobs in batches
- **Timeout Protection**: 20-second default HTTP timeout
- **Rate Limiting**: Built-in delays between API calls

## CSV Format

The output CSV includes these fields:

| Column | Type | Description |
|--------|------|-------------|
| Job_ID | string | Unique job identifier |
| Title | string | Job title |
| Company | string | Hiring company |
| Level | string | Experience level |
| Salary | string | Salary information |
| Experience_Years | string | Required years |
| Skills | string | Required skills |
| Source | string | Data source |
| Country | string | Job location country |
| City | string | Job location city |
| Location | string | Full location string |
| Work_Type | string | Remote/On-site/Hybrid |
| Apply_URL | string | Application link |
| Date | datetime | Job listing date |

## Common Use Cases

### Fetch Latest Egypt Tech Jobs
```csharp
dotnet run
```
This loads existing jobs, fetches new ones from Jooble, filters them, and saves to CSV.

### Filter Specific Roles
Edit `JoobleSearchKeywords` in `AppSettings.cs`:
```csharp
JoobleSearchKeywords = new()
{
    "oracle developer",
    "sql server developer",
    "database architect"
};
```

### Change Output Location
Modify the `CsvService` constructor:
```csharp
var csvService = new CsvService("/path/to/output/Egypt_Tech_Jobs.csv");
```

## Troubleshooting

### No jobs found
- Check internet connectivity
- Verify Jooble API key is valid
- Ensure search keywords match available positions
- Check that Egypt filter is not too restrictive

### API Rate Limiting
- Reduce `JoobleMaxPages` in AppSettings
- Increase delay in `await Task.Delay(100);` in Program.cs
- Reduce `MaxWorkers` value

### CSV Write Errors
- Ensure output directory is writable
- Check disk space availability
- Verify no other process is locking the file

## Differences from Python Version

| Feature | Python | .NET 10 |
|---------|--------|---------|
| Type Safety | Dynamic | Strict |
| Performance | Moderate | High |
| Async Support | Limited | Native |
| CSV Processing | pandas | CsvHelper |
| Concurrency | ThreadPoolExecutor | Task-based |
| Dependencies | requests, BeautifulSoup | CsvHelper |

## Future Enhancements

- [ ] Database storage (Entity Framework Core)
- [ ] Web UI (ASP.NET Core)
- [ ] Job notifications (email/webhook)
- [ ] Machine learning for job recommendations
- [ ] Multiple country support
- [ ] Advanced filtering UI

## License

Same as the original Python project.

## Contributing

Contributions are welcome! Areas for improvement:
- Additional job sources
- Better error handling
- Enhanced filtering algorithms
- Unit tests
- Performance optimizations

## Resources

- [.NET 10 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [CsvHelper Documentation](https://joshclose.github.io/CsvHelper/)
- [Jooble API Documentation](https://api.jooble.org/api/v1/positions)

## Source Policy

This conversion is limited to allowed API-style providers and curated local imports. Legacy job-board scraping references were removed because those providers do not permit this collection approach.
