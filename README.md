# Egypt Tech Jobs

A job aggregation platform that fetches tech job listings from multiple sources and displays them in a modern Angular frontend with a React admin portal.

## 🚀 Features

- **Multi-source Job Aggregation**: Fetches jobs from 8+ sources including Greenhouse, Lever, Workable, Jooble, RemoteOK, Remotive, Himalayas, and Jobicy
- **Smart Filtering**: Filter jobs by title, company, city, experience level, work type, and source
- **Blocked Companies/Keywords**: Admin can block specific companies or keywords to hide jobs from public listings
- **Google-style Pagination**: Easy navigation through job listings
- **Dark/Light Theme**: Toggle between dark and light modes with persistent preference
- **Statistics Dashboard**: View job distribution by level, city, work type, and source
- **Admin Portal**: React-based admin dashboard for managing jobs and filters
- **PostgreSQL Database**: Persistent storage for jobs, filters, and admin users

## 📁 Project Structure

```
TechJobs/
├── docker-compose.yml              # PostgreSQL database container
├── dotnet/
│   ├── EgyptTechJobsApi/           # .NET 10 Main API (port 5200)
│   │   ├── Controllers/
│   │   │   ├── JobsController.cs   # Job CRUD and search endpoints
│   │   │   ├── FetchController.cs  # Job fetching endpoints
│   │   │   └── StatisticsController.cs
│   │   ├── Services/
│   │   │   ├── JobService.cs       # Job data management
│   │   │   └── JobFetchService.cs  # Multi-source job fetching
│   │   └── Infrastructure/
│   │       └── Repositories/       # PostgreSQL data access
│   └── EgyptTechJobsAdmin/         # .NET 8 Admin API (port 5100)
│       ├── Controllers/
│       │   ├── AuthController.cs   # Authentication
│       │   ├── DashboardController.cs
│       │   ├── JobsController.cs   # Admin job management
│       │   ├── FiltersController.cs # Blocked companies/keywords
│       │   ├── PublicJobsController.cs
│       │   └── SyncController.cs   # Fetch & sync jobs
│       ├── Data/
│       │   └── ApplicationDbContext.cs
│       └── Services/
├── frontend/                       # Angular 21 Public Frontend (port 4200)
│   └── src/app/
│       ├── components/
│       │   ├── home/               # Landing page
│       │   ├── job-list/           # Job listings with filters
│       │   └── stats/              # Statistics dashboard
│       └── services/
└── frontend/admin-portal/          # React Admin Portal (port 3000)
    └── src/
        ├── pages/
        │   ├── Dashboard.tsx
        │   ├── Jobs.tsx
        │   └── Filters.tsx         # Blocked companies/keywords management
        └── services/
```

## 🛠️ Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for Main API)
- [.NET 8 SDK](https://dotnet.microsoft.com/download) (for Admin API)
- [Node.js 18+](https://nodejs.org/) (LTS recommended)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)

---

## 🗄️ Database Setup

### Step 1: Start PostgreSQL with Docker

```bash
# From the project root directory
docker-compose up -d
```

This will start a PostgreSQL 16 container with:
- **Host**: localhost
- **Port**: 5433
- **Database**: techjobs_admin
- **Username**: postgres
- **Password**: postgres

### Step 2: Verify Database is Running

```bash
docker ps
# Should show: techjobs-postgres container running
```

### Step 3: Apply Database Migrations

```bash
# Navigate to the Admin API directory
cd dotnet/EgyptTechJobsAdmin

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Apply migrations to create database schema
dotnet ef database update
```

This creates the following tables:
- `Jobs` - Job listings
- `AdminUsers` - Admin portal users
- `BlockedCompanies` - Companies to filter out
- `BlockedKeywords` - Keywords to filter out

### Connection String

Both APIs use the same connection string (configured in `appsettings.json`):

```
Host=localhost;Port=5433;Database=techjobs_admin;Username=postgres;Password=postgres
```

---

## 🚀 Running All Projects

### Quick Start (All Services)

Open 4 terminal windows and run:

**Terminal 1 - PostgreSQL Database:**
```bash
docker-compose up -d
```

**Terminal 2 - Main API (.NET 10 - Port 5200):**
```bash
cd dotnet/EgyptTechJobsApi
dotnet run --urls http://localhost:5200
```

**Terminal 3 - Admin API (.NET 8 - Port 5100):**
```bash
cd dotnet/EgyptTechJobsAdmin
dotnet run --urls http://localhost:5100
```

**Terminal 4 - Angular Frontend (Port 4200):**
```bash
cd frontend
npm install  # First time only
npm start
```

**Terminal 5 - React Admin Portal (Port 3000):**
```bash
cd frontend/admin-portal
npm install  # First time only
npm run dev
```

### Access Points

| Service | URL | Description |
|---------|-----|-------------|
| Public Frontend | http://localhost:4200 | Job listings for visitors |
| Admin Portal | http://localhost:3000 | Admin dashboard |
| Main API | http://localhost:5200/swagger | API documentation |
| Admin API | http://localhost:5100/swagger | Admin API docs |

### Default Admin Credentials

```
Email: diaadawood@techjobs.com
Password: Admin@123
```

---

## 🔄 Fetching Jobs

Jobs are fetched from external sources and stored in the PostgreSQL database.

### From Admin Portal (Recommended)
1. Login to Admin Portal at http://localhost:3000
2. Navigate to **Filters** page
3. Click **Fetch Jobs** tab
4. Click **Fetch Jobs from Sources** button

### From API Directly
```bash
# Fetch jobs and sync to database
curl -X POST http://localhost:5100/api/sync/fetch-and-sync \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 📡 API Endpoints

### Main API (Port 5200)

#### Jobs

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/jobs` | Get all jobs |
| GET | `/api/jobs/paged` | Get paginated jobs with filters |
| GET | `/api/jobs/{id}` | Get job by ID |
| GET | `/api/jobs/search` | Search jobs by title/company |
| GET | `/api/jobs/stats` | Get job statistics |
| GET | `/api/jobs/count` | Get total job count |

#### Fetch

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/fetch` | Fetch from all selected sources |
| POST | `/api/fetch/{source}` | Fetch from a specific source |
| GET | `/api/fetch/sources` | Get available job sources |

### Admin API (Port 5100)

#### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login and get JWT token |
| POST | `/api/auth/logout` | Logout |
| GET | `/api/auth/me` | Get current user info |

#### Dashboard

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard/stats` | Get dashboard statistics |

#### Filters (Blocked Companies/Keywords)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/filters/companies` | Get blocked companies |
| POST | `/api/filters/companies` | Add blocked company |
| DELETE | `/api/filters/companies/{id}` | Remove blocked company |
| GET | `/api/filters/keywords` | Get blocked keywords |
| POST | `/api/filters/keywords` | Add blocked keyword |
| DELETE | `/api/filters/keywords/{id}` | Remove blocked keyword |

#### Sync

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sync/fetch-and-sync` | Fetch jobs and sync to database |

### Query Parameters for `/api/jobs/paged`

- `title` - Filter by job title
- `company` - Filter by company name
- `city` - Filter by city
- `level` - Filter by experience level (Junior, Mid, Senior, etc.)
- `workType` - Filter by work type (Remote, Hybrid, On-site)
- `source` - Filter by job source
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 20)

---

## 🚫 Blocking Companies/Keywords

The admin portal allows blocking specific companies or keywords to hide jobs from public listings:

1. Go to **Filters** page in Admin Portal
2. **Blocked Companies** tab: Add company names to block
3. **Blocked Keywords** tab: Add keywords (jobs with these in title are hidden)

---

## 🎨 Theme Support

The application supports both light and dark themes:
- Click the 🌙/☀️ button in the navbar to toggle
- Theme preference is saved in localStorage

---

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

---

## 🔧 Configuration

### Database Connection

Edit `appsettings.json` in both API projects:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=techjobs_admin;Username=postgres;Password=postgres"
  }
}
```

### Docker Database Settings

Edit `docker-compose.yml`:
```yaml
environment:
  POSTGRES_USER: postgres
  POSTGRES_PASSWORD: postgres
  POSTGRES_DB: techjobs_admin
ports:
  - "5433:5432"  # External:Internal
```

---

## 🧹 Maintenance Commands

### Reset Database
```bash
# Stop containers and remove volumes
docker-compose down -v

# Start fresh
docker-compose up -d

# Re-apply migrations
cd dotnet/EgyptTechJobsAdmin
dotnet ef database update
```

### View Database Logs
```bash
docker logs techjobs-postgres
```

### Connect to Database (psql)
```bash
docker exec -it techjobs-postgres psql -U postgres -d techjobs_admin
```

---

## 📝 License

This project is for educational purposes.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
