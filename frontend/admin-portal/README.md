# Admin Portal - Angular

This is the Angular version of the TechJobs Admin Portal, converted from React.

## Features

- **Dashboard**: View job statistics and analytics
- **Job Management**: Create, edit, delete, and manage job visibility
- **Filters**: Block companies and keywords to filter unwanted jobs
- **Job Fetching**: Fetch jobs from external sources (Greenhouse, Lever, Workable, etc.)
- **Authentication**: Secure login with JWT tokens

## Project Structure

```
src/
├── app/
│   ├── components/
│   │   └── layout/              # Main layout with sidebar navigation
│   ├── guards/
│   │   └── auth.guard.ts        # Route protection
│   ├── interceptors/
│   │   └── auth.interceptor.ts  # HTTP interceptor for auth tokens
│   ├── models/
│   │   └── job.model.ts         # TypeScript interfaces
│   ├── pages/
│   │   ├── dashboard/           # Dashboard with statistics
│   │   ├── job-list/            # Job listing with filters
│   │   ├── create-job/          # Create new job form
│   │   ├── edit-job/            # Edit existing job form
│   │   ├── filters/             # Blocked companies/keywords & fetch
│   │   └── login/               # Authentication page
│   ├── services/
│   │   ├── api.service.ts       # API calls
│   │   └── auth.service.ts      # Authentication logic
│   ├── app.component.ts
│   ├── app.config.ts
│   └── app.routes.ts
├── environments/
│   ├── environment.ts
│   └── environment.prod.ts
├── index.html
├── main.ts
└── styles.scss
```

## Development

### Prerequisites

- Node.js 18+
- npm or yarn
- Angular CLI 19+

### Installation

```bash
npm install
```

### Development Server

```bash
npm start
# or
ng serve
```

Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

### Build

```bash
npm run build
# or
ng build
```

The build artifacts will be stored in the `dist/` directory.

### API Configuration

The API URL is configured in `src/environments/environment.ts`. Default is `http://localhost:5100`.

## Authentication

The admin portal uses JWT authentication. Admin accounts:
- diaadawood@techjobs.com
- mohamedabdelmohsen@techjobs.com
- marwanemad@techjobs.com

Default password: `Admin@123`

## Technologies

- Angular 19 (standalone components)
- TypeScript
- Tailwind CSS
- RxJS
- Angular Router
- Angular HttpClient
