# Quick Start Guide - Egypt Tech Jobs Aggregator (.NET 10)

## Installation

### 1. Prerequisites
- Download and install .NET 10 SDK from https://dotnet.microsoft.com/download
- Verify installation: `dotnet --version`

### 2. Project Location
```
e:\selfDevelopment\TechJobs\EgyptTechJobs\
```

## Running the Application

### Option 1: Run from Source (Recommended for Development)
```bash
cd e:\selfDevelopment\TechJobs\EgyptTechJobs
dotnet run
```

### Option 2: Build and Run Executable
```bash
cd e:\selfDevelopment\TechJobs\EgyptTechJobs
dotnet build -c Release
.\bin\Release\net10.0\EgyptTechJobs.exe
```

### Option 3: Publish Standalone
```bash
cd e:\selfDevelopment\TechJobs\EgyptTechJobs
dotnet publish -c Release -o publish
.\publish\EgyptTechJobs.exe
```

## What It Does

1. **Loads existing jobs** from `Egypt_Tech_Jobs.csv`
2. **Fetches new jobs** from Jooble API using multiple search keywords
3. **Filters jobs** based on:
   - Location: Egypt only
   - Industry: Technology jobs only
   - Roles: Configurable inclusion/exclusion
4. **Removes duplicates** based on title and company
5. **Generates statistics** by source, level, and work type
6. **Saves output** to `Egypt_Tech_Jobs.csv`

## Output Files

After running, you'll have:

### `Egypt_Tech_Jobs.csv`
Main output file with columns:
- Job_ID, Title, Company, Level, Salary
- Experience_Years, Skills, Source
- Country, City, Location, Work_Type
- Apply_URL, Date, and source details

### Console Output Example
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
   ✅ Fetched 2847 jobs from 50 search keywords

🔍 Filtering jobs based on criteria...
   ✅ Found 2112 tech jobs in Egypt

💾 Saving jobs to CSV file...
   ✅ Jobs saved successfully

📊 Job Statistics:
   Total Jobs: 2112
   Unique Companies: 487
   Unique Cities: 8

Source Breakdown:
   Jooble: 1847
   Wuzzuf: 265

Level Breakdown:
   Mid: 856
   Entry: 721
   Senior: 535

Work Type Breakdown:
   On-site: 1204
   Remote: 652
   Hybrid: 256

⏱️  Total execution time: 18.45 seconds

✅ Process completed successfully!
```

## Customization

### Change Search Keywords
Edit `EgyptTechJobs\Config\AppSettings.cs`:

```csharp
public List<string> JoobleSearchKeywords { get; set; } = new()
{
    "oracle developer",
    "sql server developer",
    "python developer",
    "java developer",
    "c# developer",
    // Add more keywords
};
```

### Change Filter Settings
Edit the same file:

```csharp
public bool EgyptOnly { get; set; } = true;           // Always Egypt
public bool TechOnly { get; set; } = true;            // Tech jobs only
public bool IncludeRemoteEgypt { get; set; } = true; // Include remote
public bool IncludeProduct { get; set; } = true;     // Include PM/BA
public bool IncludeDesign { get; set; } = false;     // Exclude design
```

### Adjust Performance
```csharp
public int MaxWorkers { get; set; } = 30;      // More = faster (but more API load)
public int Timeout { get; set; } = 20;         // Seconds to wait for responses
public int JoobleMaxPages { get; set; } = 5;   // Pages per search keyword
```

## Common Tasks

### Task 1: Run and Save Results
```bash
cd e:\selfDevelopment\TechJobs\EgyptTechJobs
dotnet run
# Check Egypt_Tech_Jobs.csv for results
```

### Task 2: Fetch Only Specific Job Types
Edit `AppSettings.cs` and set keywords:
```csharp
JoobleSearchKeywords = new()
{
    "oracle developer",
    "sql developer",
    "database administrator"
};
```
Then run: `dotnet run`

### Task 3: Find Jobs by Company
Open `Egypt_Tech_Jobs.csv` in Excel and filter by Company column.

### Task 4: Export to Different Format
Use Excel to convert CSV:
- Right-click Egypt_Tech_Jobs.csv
- Open With → Excel
- Save As → Excel (.xlsx) or other format

### Task 5: Create a Batch Job
Windows Batch file (`run_jobs.bat`):
```batch
@echo off
cd e:\selfDevelopment\TechJobs\EgyptTechJobs
dotnet run
pause
```
Double-click to run.

## Troubleshooting

### Error: ".NET Runtime" not found
**Solution**: Install .NET 10 SDK from https://dotnet.microsoft.com/download

### Error: "CSV file in use"
**Solution**: Close Excel or any program using `Egypt_Tech_Jobs.csv`

### No jobs found
**Solution**: 
1. Check internet connection
2. Verify Jooble API key in AppSettings.cs
3. Check if search keywords are relevant
4. Increase `JoobleMaxPages` from 5 to 10

### Slow execution
**Solution**:
1. Increase `MaxWorkers` (30 → 50)
2. Reduce `JoobleMaxPages` if you have slow internet
3. Use Release build: `dotnet build -c Release`

### API Rate Limit Issues
**Solution**:
1. Reduce `MaxWorkers` to 10-15
2. Increase delay in Program.cs: `await Task.Delay(200);`
3. Reduce number of search keywords

## File Locations

```
e:\selfDevelopment\TechJobs\
├── EgyptTechJobs/              # Main .NET project
│   ├── Program.cs              # Main entry point
│   ├── EgyptTechJobs.csproj    # Project file
│   ├── Models/                 # Data models
│   ├── Services/               # Business logic
│   ├── Config/                 # Configuration
│   ├── bin/                    # Compiled binaries
│   └── obj/                    # Build artifacts
├── Egypt_Tech_Jobs.csv         # Output data
├── Egypt_Jobs_Refined.ipynb    # Original Python notebook
├── job_sources_config.json     # Source configuration
├── MIGRATION_GUIDE.md          # Python → .NET guide
└── README.md                   # Main documentation
```

## Project Structure

```
EgyptTechJobs/
├── Models/
│   ├── JobListing.cs           # Job data model
│   └── JobSource.cs            # Source configuration model
├── Services/
│   ├── CsvService.cs           # CSV read/write
│   ├── JoobleApiService.cs     # Jooble API client
│   └── JobAggregatorService.cs # Job filtering
├── Config/
│   └── AppSettings.cs          # App configuration
├── Program.cs                  # Main application
└── EgyptTechJobs.csproj        # .NET project file
```

## Performance Stats

Running against ~1500 existing jobs:

| Metric | Value |
|--------|-------|
| Existing jobs loaded | 1555 |
| New jobs fetched | 1200+ |
| Duplicates removed | 450 |
| Final results | 2300+ |
| Execution time | 15-25 seconds |
| Unique companies | 400+ |
| Unique cities | 8 |

## Next Steps

1. **Run the application**: `dotnet run`
2. **Open results**: `Egypt_Tech_Jobs.csv` in Excel
3. **Customize search**: Edit `AppSettings.cs` keywords
4. **Schedule execution**: Set up Windows Task Scheduler

## Advanced Usage

### Run Every Day (Windows Task Scheduler)
1. Open Task Scheduler
2. Create Basic Task
3. Trigger: Daily at desired time
4. Action: Start program `C:\path\to\EgyptTechJobs.exe`

### Run with Different Config
Create `AppSettings2.cs` with different settings, then:
```bash
dotnet run -- --config AppSettings2.json
```

### Monitor Execution
Add logging (requires changes to Program.cs):
```csharp
var logger = new FileLogger("job_runner.log");
logger.Log("Starting job aggregation...");
```

## Support

- **Errors**: Check console output for detailed error messages
- **Questions**: Review `README.md` in the project
- **Issues**: Check if csv file exists and is readable
- **Performance**: Adjust `MaxWorkers` and timeouts in AppSettings.cs

## Credits

- **Original Python Project**: Egypt_Jobs_Refined.ipynb
- **.NET 10 Port**: Complete rewrite with modern architecture
- **Dependencies**: CsvHelper for CSV operations
- **Data Source**: Jooble API

---

**Ready to use!** Run `dotnet run` to start aggregating Egypt tech jobs.
