# Running Guide - Egypt Tech Jobs

This guide explains how to run the project in both **Development** and **Production** environments.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local development)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)

---

## Environment Overview

| Service | Dev Port | Prod Port | Description |
|---------|----------|-----------|-------------|
| Jobs API | 5200 | 5200 | Main job fetching API |
| Admin API | 5203 | 5203 | Admin portal backend |
| Jobs Frontend | 4200 | 4200 | Public job listings |
| Admin Portal | 4201 | 4201 | Admin dashboard |
| PostgreSQL | 5433 | Supabase | Database |

---

## Development Environment

### 1. Start PostgreSQL Database (Docker)

```powershell
cd e:\selfDevelopment\TechJobs
docker-compose up -d postgres
```

Verify it's running:
```powershell
docker ps | Select-String "techjobs"
```

### 2. Start Backend Services

**Terminal 1 - Admin API (Development):**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsAdmin
dotnet run --urls http://localhost:5203
```

**Terminal 2 - Jobs API (Development):**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsApi
dotnet run --urls http://localhost:5200
```

### 3. Start Frontend Services

**Terminal 3 - Jobs Frontend:**
```powershell
cd e:\selfDevelopment\TechJobs\frontend
npm install  # First time only
npm start
```

**Terminal 4 - Admin Portal:**
```powershell
cd e:\selfDevelopment\TechJobs\frontend\admin-portal
npm install  # First time only
ng serve --port 4201
```

### Development URLs

- Jobs Portal: http://localhost:4200
- Admin Portal: http://localhost:4201
- Admin API Swagger: http://localhost:5203/swagger
- Jobs API Swagger: http://localhost:5200/swagger

### Development Database

Local PostgreSQL via Docker:
- Host: `localhost`
- Port: `5433`
- Database: `techjobs_admin`
- Username: `postgres`
- Password: `postgres`

---

## Production Environment

### 1. Configure Production Settings

Create `appsettings.Production.json` files (these are gitignored):

**`dotnet/EgyptTechJobsAdmin/appsettings.Production.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=<your-supabase-pooler-host>;Port=5432;Database=postgres;Username=<your-username>;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Key": "<your-jwt-secret-key>",
    "Issuer": "TechJobsAdmin",
    "Audience": "TechJobsAdminPortal"
  }
}
```

**`dotnet/EgyptTechJobsApi/appsettings.Production.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=<your-supabase-pooler-host>;Port=5432;Database=postgres;Username=<your-username>;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 2. Supabase Connection Strings

For **IPv4 networks**, use the **Session Pooler**:
```
Host=aws-1-eu-north-1.pooler.supabase.com
Port=5432
Username=postgres.<project-ref>
```

For **IPv6 networks**, use **Direct Connection**:
```
Host=db.<project-ref>.supabase.co
Port=5432
Username=postgres
```

### 3. Start Production Backend Services

**Terminal 1 - Admin API (Production):**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsAdmin
dotnet run --urls http://localhost:5203
```

**Terminal 2 - Jobs API (Production):**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsApi
dotnet run --urls http://localhost:5200
```

### 4. Start Frontend Services

Same as Development:
```powershell
# Terminal 3 - Jobs Frontend
cd e:\selfDevelopment\TechJobs\frontend
npm start

# Terminal 4 - Admin Portal
cd e:\selfDevelopment\TechJobs\frontend\admin-portal
ng serve --port 4201
```

---

## Quick Start Scripts

### Run All Services (Development)

```powershell
# Start Docker PostgreSQL
docker-compose up -d postgres

# Start Admin API (new terminal)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$env:ASPNETCORE_ENVIRONMENT='Development'; cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsAdmin; dotnet run --urls http://localhost:5203"

# Start Jobs API (new terminal)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$env:ASPNETCORE_ENVIRONMENT='Development'; cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsApi; dotnet run --urls http://localhost:5200"

# Start Jobs Frontend (new terminal)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd e:\selfDevelopment\TechJobs\frontend; npm start"

# Start Admin Portal (new terminal)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd e:\selfDevelopment\TechJobs\frontend\admin-portal; ng serve --port 4201"
```

### Run All Services (Production)

```powershell
# Start Admin API (new terminal)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$env:ASPNETCORE_ENVIRONMENT='Production'; cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsAdmin; dotnet run --urls http://localhost:5203"

# Start Jobs API (new terminal)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$env:ASPNETCORE_ENVIRONMENT='Production'; cd e:\selfDevelopment\TechJobs\dotnet\EgyptTechJobsApi; dotnet run --urls http://localhost:5200"

# Start Frontends (same commands as development)
```

---

## Troubleshooting

### Port Already in Use

```powershell
# Find and kill process on a specific port
netstat -ano | findstr ":5203"
taskkill /F /PID <PID>
```

### Database Connection Failed

1. **Development**: Ensure Docker is running: `docker ps`
2. **Production**: Verify Supabase project is active and using correct pooler endpoint

### CORS Errors

Allowed origins are configured in `Program.cs`. Add new origins if needed:
```csharp
policy.WithOrigins("http://localhost:4200", "http://localhost:4201", ...)
```

---

## Default Admin Credentials

After first run, default admin users are seeded:
- Email: `admin@techjobs.com`
- Password: `Admin123!`

---

## API Endpoints

### Admin API (5203)
- `POST /api/auth/login` - Authenticate
- `GET /api/auth/me` - Get current user
- `GET /api/dashboard/stats` - Dashboard statistics
- `GET /api/jobs` - List jobs (admin)
- `GET /api/public/jobs` - List visible jobs (public)
- `GET /api/filters/blocked-companies` - Blocked companies
- `GET /api/filters/blocked-keywords` - Blocked keywords

### Jobs API (5200)
- `GET /api/jobs` - List all jobs
- `POST /api/fetch` - Fetch jobs from all sources
- `POST /api/fetch/{source}` - Fetch from specific source
- `GET /api/statistics` - Job statistics
