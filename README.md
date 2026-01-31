# Egypt Tech Jobs

A job aggregation platform that fetches tech job listings from multiple sources and displays them in a modern Angular frontend.

## 🚀 Features

- **Multi-source Job Aggregation**: Fetches jobs from 8+ sources including Greenhouse, Lever, Workable, Jooble, RemoteOK, Remotive, Himalayas, and Jobicy
- **Smart Filtering**: Filter jobs by title, company, city, experience level, work type, and source
- **Google-style Pagination**: Easy navigation through job listings
- **Dark/Light Theme**: Toggle between dark and light modes with persistent preference
- **Statistics Dashboard**: View job distribution by level, city, work type, and source
- **Real-time Fetching**: Fetch fresh job listings from all sources with a single click

## 📁 Project Structure

```
TechJobs/
├── dotnet/
│   └── EgyptTechJobsApi/          # .NET 10 Backend API
│       ├── Controllers/
│       │   ├── JobsController.cs   # Job CRUD and search endpoints
│       │   └── FetchController.cs  # Job fetching endpoints
│       ├── Services/
│       │   ├── JobService.cs       # Job data management
│       │   └── JobFetchService.cs  # Multi-source job fetching
│       └── Models/
│           └── Job.cs              # Job model
├── frontend/                       # Angular 18+ Frontend
│   └── src/
│       └── app/
│           ├── components/
│           │   ├── home/           # Landing page
│           │   ├── job-list/       # Job listings with filters
│           │   ├── fetch-jobs/     # Fetch jobs UI
│           │   └── stats/          # Statistics dashboard
│           ├── services/
│           │   ├── job.service.ts  # API communication
│           │   └── theme.service.ts # Dark/light theme
│           └── models/
│               └── job.model.ts    # TypeScript interfaces
├── data/
│   └── Egypt_Tech_Jobs.csv         # Job data storage
└── job_sources_config.json         # Job sources configuration
```

## 🛠️ Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) (LTS recommended)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)

## 🚀 Getting Started

### Option 1: Run Both Servers Manually

#### Start the Backend API

```bash
# Navigate to the API directory
cd dotnet/EgyptTechJobsApi

# Restore dependencies and run
dotnet run
```

The API will start on **http://localhost:5200**

#### Start the Frontend

```bash
# Navigate to the frontend directory
cd frontend

# Install dependencies (first time only)
npm install

# Start the development server
npm start
```

The frontend will start on **http://localhost:4200**

### Option 2: Quick Start (Windows PowerShell)

Open two terminal windows:

**Terminal 1 - Backend:**
```powershell
cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsApi
dotnet run
```

**Terminal 2 - Frontend:**
```powershell
cd e:\selfDevelopment\TechJobs\frontend
npm start
```

## 📡 API Endpoints

### Jobs

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/jobs` | Get all jobs |
| GET | `/api/jobs/paged` | Get paginated jobs with filters |
| GET | `/api/jobs/{id}` | Get job by ID |
| GET | `/api/jobs/search` | Search jobs by title/company |
| GET | `/api/jobs/stats` | Get job statistics |
| GET | `/api/jobs/count` | Get total job count |

### Fetch

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/fetch` | Fetch from all selected sources |
| POST | `/api/fetch/{source}` | Fetch from a specific source |
| GET | `/api/fetch/sources` | Get available job sources |

### Query Parameters for `/api/jobs/paged`

- `title` - Filter by job title
- `company` - Filter by company name
- `city` - Filter by city
- `level` - Filter by experience level (Junior, Mid, Senior, etc.)
- `workType` - Filter by work type (Remote, Hybrid, On-site)
- `source` - Filter by job source
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 20)

## 🎨 Theme Support

The application supports both light and dark themes:
- Click the 🌙/☀️ button in the navbar to toggle
- Theme preference is saved in localStorage

## 📊 Job Sources

| Source | Type | Rate Limit |
|--------|------|------------|
| Greenhouse | ATS | 60 RPM |
| Lever | ATS | 60 RPM |
| Workable | ATS | 60 RPM |
| Jooble | API | 500/day |
| RemoteOK | API | 60 RPM |
| Remotive | API | 60 RPM |
| Himalayas | API | 60 RPM |
| Jobicy | API | 60 RPM |

## 🔧 Configuration

### Backend Port

Edit `dotnet/EgyptTechJobsApi/Properties/launchSettings.json`:
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5200"
    }
  }
}
```

### Frontend API URL

Edit `frontend/src/app/services/job.service.ts`:
```typescript
private readonly apiUrl = 'http://localhost:5200/api';
```

## 📝 License

This project is for educational purposes.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
