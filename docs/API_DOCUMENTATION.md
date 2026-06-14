# Egypt Tech Jobs API - Documentation

## Project Overview

A .NET 10 Web API that serves Egyptian tech job listings from a CSV data source. The API provides endpoints for browsing, searching, and analyzing job data with Swagger documentation for testing.

---

## Current Project State ✅

| Component | Status | Notes |
|-----------|--------|-------|
| Console App | ✅ Complete | `dotnet/EgyptTechJobs/` |
| Web API | ✅ Complete | `dotnet/EgyptTechJobsApi/` |
| CSV Data Loading | ✅ Working | 1553 jobs loaded |
| Swagger UI | ✅ Working | Available at `/swagger` |
| Error Handling | ✅ Basic | CSV parsing errors handled |

---

## Project Structure

```
TechJobs/
├── data/
│   └── Egypt_Tech_Jobs.csv          # Source data (1553 jobs)
├── docs/
│   └── API_DOCUMENTATION.md         # This file
├── dotnet/
│   ├── EgyptTechJobs/               # Console Application
│   │   ├── Models/
│   │   │   └── JobListing.cs
│   │   ├── Services/
│   │   │   └── JobService.cs
│   │   ├── Config/
│   │   │   └── AppConfig.cs
│   │   ├── Program.cs
│   │   └── EgyptTechJobs.csproj
│   │
│   └── EgyptTechJobsApi/            # Web API
│       ├── Controllers/
│       │   ├── JobsController.cs
│       │   └── StatisticsController.cs
│       ├── Models/
│       │   ├── JobListing.cs
│       │   └── ApiResponse.cs
│       ├── Services/
│       │   └── JobService.cs
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Program.cs
│       └── EgyptTechJobsApi.csproj
├── python/
│   └── Egypt_Jobs_Refined.ipynb     # Original Python notebook
└── README.md
```

---

## API Endpoints

### Base URL
```
http://localhost:5200
```

### Swagger UI
```
http://localhost:5200/swagger
```

---

### Jobs Controller (`/api/jobs`)

#### 1. Get All Jobs (Paginated)
```http
GET /api/jobs?page=1&pageSize=20
```

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page (max 100) |

**Response:**
```json
{
  "success": true,
  "message": "Found 1553 jobs",
  "data": [
    {
      "jobId": "-6687376651442586105",
      "title": "Design engineer",
      "company": "Al ayat for printing and packaging",
      "level": "Mid",
      "salary": "",
      "experienceYears": "",
      "skills": "",
      "country": "Egypt",
      "city": "Giza",
      "workType": "On-site",
      "location": "Giza, Egypt",
      "date": "2026-01-23T00:00:00"
    }
  ],
  "timestamp": "2026-01-30T12:00:00Z"
}
```

---

#### 2. Get Job by ID
```http
GET /api/jobs/{id}
```

**Response:**
```json
{
  "success": true,
  "message": "Job found",
  "data": { /* JobListing object */ },
  "timestamp": "2026-01-30T12:00:00Z"
}
```

---

#### 3. Search Jobs
```http
GET /api/jobs/search?keyword=engineer&page=1&pageSize=20
```

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| keyword | string | Yes | Search term (searches title, company, skills) |
| page | int | No | Page number (default: 1) |
| pageSize | int | No | Items per page (default: 20) |

---

#### 4. Filter Jobs by Level
```http
GET /api/jobs/level/{level}
```

**Valid Levels:** `Junior`, `Mid`, `Senior`, `Lead`, `Principal`, `Intern`

---

#### 5. Filter Jobs by Work Type
```http
GET /api/jobs/worktype/{workType}
```

**Valid Work Types:** `Remote`, `On-site`, `Hybrid`, `Relocation Required`

---

#### 6. Filter Jobs by Country
```http
GET /api/jobs/country/{country}
```

**Examples:** `Egypt`, `Remote`, `USA`, `UAE`

---

### Statistics Controller (`/api/statistics`)

#### 1. Get Overview Statistics
```http
GET /api/statistics/overview
```

**Response:**
```json
{
  "success": true,
  "message": "Statistics overview",
  "data": {
    "totalJobs": 1553,
    "uniqueCompanies": 150,
    "uniqueCountries": 25,
    "jobsByLevel": {
      "Senior": 450,
      "Mid": 600,
      "Junior": 200,
      "Lead": 150,
      "Principal": 50,
      "Intern": 103
    },
    "jobsByWorkType": {
      "Remote": 800,
      "On-site": 500,
      "Hybrid": 150,
      "Relocation Required": 103
    }
  },
  "timestamp": "2026-01-30T12:00:00Z"
}
```

---

#### 2. Get Jobs by Source
```http
GET /api/statistics/sources
```

---

#### 3. Get Top Companies
```http
GET /api/statistics/top-companies?count=10
```

---

## Running the API

### Prerequisites
- .NET 10 SDK
- CSV data file at `e:\selfDevelopment\TechJobs\data\Egypt_Tech_Jobs.csv`

### Start the API
```powershell
cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsApi
dotnet run --urls http://localhost:5200
```

### Build for Production
```powershell
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

---

## Data Model

### JobListing

```csharp
public class JobListing
{
    public string JobId { get; set; }           // Unique identifier
    public string Title { get; set; }           // Job title
    public string Company { get; set; }         // Company name
    public string Level { get; set; }           // Junior/Mid/Senior/Lead/Principal/Intern
    public string Salary { get; set; }          // Salary info (often empty)
    public string ExperienceYears { get; set; } // Required experience
    public string Skills { get; set; }          // Required skills (comma-separated)
    public string Source { get; set; }          // Job board source
    public string SourceId { get; set; }        // Source identifier
    public string SourceType { get; set; }      // API type
    public string AllowedMode { get; set; }     // Display mode
    public string AttributionRequired { get; set; }
    public string SourceUrl { get; set; }       // Source website
    public int RateLimitRpm { get; set; }       // Rate limit
    public int RateLimitBurst { get; set; }     // Burst limit
    public string TakedownContact { get; set; } // Contact for takedown
    public string TermsUrl { get; set; }        // Terms of service URL
    public string SourceNotes { get; set; }     // Additional notes
    public string Country { get; set; }         // Country
    public string City { get; set; }            // City
    public string WorkType { get; set; }        // Remote/On-site/Hybrid
    public string Location { get; set; }        // Full location string
    public string ApplyUrl { get; set; }        // Application URL
    public DateTime? Date { get; set; }         // Posted date (nullable)
}
```

---

## Configuration

### launchSettings.json
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5200",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### CSV Path Configuration
Currently hardcoded in `JobService.cs`:
```csharp
private readonly string _csvPath = @"e:\selfDevelopment\TechJobs\data\Egypt_Tech_Jobs.csv";
```

---

## Known Issues & Fixes Applied

### 1. CSV Date Parsing
- **Issue:** Some dates in CSV were empty or malformed
- **Fix:** Made `Date` property nullable (`DateTime?`)

### 2. CSV Header Mapping
- **Issue:** CSV headers didn't match property names
- **Fix:** Added `[Name("Header_Name")]` attributes to model properties

### 3. CSV Error Handling
- **Issue:** Strict validation caused parsing failures
- **Fix:** Added error handlers in CsvConfiguration:
```csharp
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = true,
    MissingFieldFound = null,
    HeaderValidated = null,
    BadDataFound = null
};
```

---

## Future Enhancements 🚀

### Priority 1: High Impact

#### 1. Add Database Support
Replace CSV with database for better performance and scalability.
```
- Add Entity Framework Core
- Create migrations
- Add repository pattern
- Support for SQL Server or PostgreSQL
```

#### 2. Add Caching
Implement response caching for frequently accessed endpoints.
```csharp
// Add to Program.cs
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

// Add to controllers
[ResponseCache(Duration = 300)]
public async Task<IActionResult> GetJobs()
```

#### 3. Add Authentication
Secure the API with JWT authentication.
```
- Add JWT Bearer authentication
- Create user registration/login endpoints
- Add role-based authorization
- Protect sensitive endpoints
```

---

### Priority 2: Medium Impact

#### 4. Advanced Search & Filtering
```http
GET /api/jobs/advanced?
    title=engineer&
    company=google&
    skills=python,react&
    level=senior&
    workType=remote&
    minSalary=50000&
    country=egypt&
    dateFrom=2026-01-01
```

#### 5. Add Sorting
```http
GET /api/jobs?sortBy=date&sortOrder=desc
GET /api/jobs?sortBy=company&sortOrder=asc
```

#### 6. Add Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

#### 7. Add Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("csv_file", () =>
    {
        var path = @"e:\selfDevelopment\TechJobs\data\Egypt_Tech_Jobs.csv";
        return File.Exists(path)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("CSV file not found");
    });

app.MapHealthChecks("/health");
```

---

### Priority 3: Nice to Have

#### 8. Add Logging with Serilog
```csharp
builder.Host.UseSerilog((context, config) =>
{
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day);
});
```

#### 9. Add API Versioning
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Usage: /api/v1/jobs, /api/v2/jobs
```

#### 10. Add Job Alerts/Subscriptions
```
- Create endpoint to subscribe to job alerts
- Filter by keywords, company, level
- Send email notifications for new matching jobs
```

#### 11. Add Favorites/Saved Jobs
```
- User can save jobs to favorites
- Requires authentication
- Create UserFavorites table
```

#### 12. Add Application Tracking
```
- Track when user applies to a job
- Store application status
- Create ApplicationHistory table
```

---

## Code Snippets for Enhancements

### Add CORS Support
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

### Add Global Exception Handler
```csharp
// Middleware/ExceptionMiddleware.cs
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "An error occurred",
                error = ex.Message
            });
        }
    }
}
```

### Add Configuration from appsettings.json
```json
// appsettings.json
{
  "AppSettings": {
    "CsvPath": "e:\\selfDevelopment\\TechJobs\\data\\Egypt_Tech_Jobs.csv",
    "CacheExpirationMinutes": 30,
    "DefaultPageSize": 20,
    "MaxPageSize": 100
  }
}
```

```csharp
// AppSettings.cs
public class AppSettings
{
    public string CsvPath { get; set; }
    public int CacheExpirationMinutes { get; set; }
    public int DefaultPageSize { get; set; }
    public int MaxPageSize { get; set; }
}

// Program.cs
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
```

---

## Testing

### Manual Testing with curl
```powershell
# Get all jobs
curl http://localhost:5200/api/jobs

# Search for jobs
curl "http://localhost:5200/api/jobs/search?keyword=engineer"

# Get statistics
curl http://localhost:5200/api/statistics/overview
```

### PowerShell Testing
```powershell
# Get jobs
(Invoke-WebRequest -Uri "http://localhost:5200/api/jobs?pageSize=5" -UseBasicParsing).Content | ConvertFrom-Json

# Search
(Invoke-WebRequest -Uri "http://localhost:5200/api/jobs/search?keyword=data" -UseBasicParsing).Content | ConvertFrom-Json
```

---

## Dependencies

### NuGet Packages
| Package | Version | Purpose |
|---------|---------|---------|
| CsvHelper | 33.0.1 | CSV file parsing |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI |

### Future Packages to Consider
| Package | Purpose |
|---------|---------|
| Microsoft.EntityFrameworkCore | Database ORM |
| Serilog.AspNetCore | Structured logging |
| AspNetCoreRateLimit | Rate limiting |
| Microsoft.AspNetCore.Authentication.JwtBearer | JWT auth |
| FluentValidation.AspNetCore | Input validation |

---

## Contact & Maintenance

**Last Updated:** January 30, 2026
**API Version:** 1.0.0
**Target Framework:** .NET 10
**Port:** 5200

---

## Quick Start Checklist

When returning to this project:

1. ✅ Ensure CSV file exists at `data/Egypt_Tech_Jobs.csv`
2. ✅ Start the API:
   ```powershell
   cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsApi
   dotnet run --urls http://localhost:5200
   ```
3. ✅ Open Swagger UI: http://localhost:5200/swagger
4. ✅ Test endpoints to verify everything works
5. 🚀 Pick an enhancement from the list above and implement it!

---

*Happy Coding! 🎉*
