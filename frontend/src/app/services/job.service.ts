import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { 
  Job, 
  JobsResponse, 
  PagedJobsResponse, 
  FetchOptions, 
  FetchResult, 
  JobSource, 
  FilterOptions,
  StatsResponse 
} from '../models/job.model';

// Admin API response types
interface AdminJob {
  id: number;
  title: string;
  company: string;
  location?: string;
  country?: string;
  city?: string;
  workType?: string;
  applyUrl: string;
  source?: string;
  jobId?: string;
  postedDate?: string;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
  isManualEntry: boolean;
  isVisibleToUsers: boolean;
  description?: string;
  salaryRange?: string;
  tags?: string;
}

interface AdminPaginatedResponse {
  items: AdminJob[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

interface AdminStatsResponse {
  totalJobs: number;
  jobsByCountry: { [key: string]: number };
  jobsByWorkType: { [key: string]: number };
  jobsBySource: { [key: string]: number };
}

@Injectable({
  providedIn: 'root'
})
export class JobService {
  // Using Admin API's public endpoint (database-backed)
  private readonly apiUrl = 'http://localhost:5100/api/public';
  // Keep main API for fetch operations only
  private readonly csvApiUrl = 'http://localhost:5200/api';

  constructor(private http: HttpClient) {}

  /**
   * Convert Admin API job to frontend Job format
   */
  private mapAdminJobToJob(adminJob: AdminJob): Job {
    return {
      jobId: adminJob.jobId || adminJob.id.toString(),
      title: adminJob.title,
      company: adminJob.company,
      level: '',
      salary: adminJob.salaryRange || '',
      experienceYears: '',
      skills: adminJob.description || '',
      source: adminJob.source || '',
      sourceId: adminJob.jobId || '',
      sourceType: adminJob.isManualEntry ? 'Manual' : 'Fetched',
      allowedMode: '',
      attributionRequired: '',
      sourceUrl: adminJob.applyUrl,
      rateLimitRpm: 0,
      rateLimitBurst: 0,
      takedownContact: '',
      termsUrl: '',
      sourceNotes: '',
      country: adminJob.country || '',
      city: adminJob.city || '',
      workType: adminJob.workType || '',
      location: adminJob.location || '',
      applyUrl: adminJob.applyUrl,
      date: adminJob.postedDate || adminJob.createdAt
    };
  }

  /**
   * Get all jobs with optional filtering (from database)
   */
  getJobs(filters?: FilterOptions): Observable<JobsResponse> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.title) params = params.set('search', filters.title);
      if (filters.source) params = params.set('source', filters.source);
      if (filters.workType) params = params.set('workType', filters.workType);
      // Get all jobs for non-paged request
      params = params.set('pageSize', '10000');
    }

    return this.http.get<AdminPaginatedResponse>(`${this.apiUrl}/jobs`, { params }).pipe(
      map(response => ({
        success: true,
        message: 'Jobs fetched from database',
        data: response.items.map(job => this.mapAdminJobToJob(job))
      }))
    );
  }

  /**
   * Get paginated jobs with optional filtering (from database)
   */
  getPagedJobs(filters?: FilterOptions): Observable<PagedJobsResponse> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.title || filters.company) {
        params = params.set('search', filters.title || filters.company || '');
      }
      if (filters.source) params = params.set('source', filters.source);
      if (filters.workType) params = params.set('workType', filters.workType);
      if (filters.page) params = params.set('page', filters.page.toString());
      if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    }

    return this.http.get<AdminPaginatedResponse>(`${this.apiUrl}/jobs`, { params }).pipe(
      map(response => ({
        success: true,
        message: 'Jobs fetched from database',
        data: {
          items: response.items.map(job => this.mapAdminJobToJob(job)),
          totalCount: response.totalCount,
          pageNumber: response.page,
          pageSize: response.pageSize,
          totalPages: response.totalPages
        }
      }))
    );
  }

  /**
   * Get job by ID (from database)
   */
  getJobById(id: string): Observable<Job> {
    return this.http.get<AdminJob>(`${this.apiUrl}/jobs/${id}`).pipe(
      map(job => this.mapAdminJobToJob(job))
    );
  }

  /**
   * Search jobs by title and company (from database)
   */
  searchJobs(title?: string, company?: string): Observable<JobsResponse> {
    let params = new HttpParams();
    if (title || company) {
      params = params.set('search', title || company || '');
    }
    params = params.set('pageSize', '10000');

    return this.http.get<AdminPaginatedResponse>(`${this.apiUrl}/jobs`, { params }).pipe(
      map(response => ({
        success: true,
        message: 'Jobs searched from database',
        data: response.items.map(job => this.mapAdminJobToJob(job))
      }))
    );
  }

  /**
   * Get job statistics (from database)
   */
  getStats(): Observable<StatsResponse> {
    return this.http.get<AdminStatsResponse>(`${this.apiUrl}/stats`).pipe(
      map(response => ({
        totalJobs: response.totalJobs,
        byCity: {}, // Not available in admin stats
        byLevel: {},
        bySource: response.jobsBySource,
        byWorkType: response.jobsByWorkType
      }))
    );
  }

  /**
   * Fetch jobs from all sources (still uses CSV API)
   */
  fetchJobs(options?: FetchOptions): Observable<FetchResult> {
    return this.http.post<FetchResult>(`${this.csvApiUrl}/fetch`, options || {});
  }

  /**
   * Fetch jobs from a specific source (still uses CSV API)
   */
  fetchFromSource(source: string): Observable<FetchResult> {
    return this.http.post<FetchResult>(`${this.csvApiUrl}/fetch/${source}`, {});
  }

  /**
   * Get available sources (still uses CSV API)
   */
  getSources(): Observable<{ sources: JobSource[] }> {
    return this.http.get<{ sources: JobSource[] }>(`${this.csvApiUrl}/fetch/sources`);
  }
}
