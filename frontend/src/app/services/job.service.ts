import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
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

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private readonly apiUrl = 'http://localhost:5200/api';

  constructor(private http: HttpClient) {}

  /**
   * Get all jobs with optional filtering
   */
  getJobs(filters?: FilterOptions): Observable<JobsResponse> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.title) params = params.set('title', filters.title);
      if (filters.company) params = params.set('company', filters.company);
      if (filters.city) params = params.set('city', filters.city);
      if (filters.level) params = params.set('level', filters.level);
      if (filters.source) params = params.set('source', filters.source);
      if (filters.workType) params = params.set('workType', filters.workType);
    }

    return this.http.get<JobsResponse>(`${this.apiUrl}/jobs`, { params });
  }

  /**
   * Get paginated jobs with optional filtering
   */
  getPagedJobs(filters?: FilterOptions): Observable<PagedJobsResponse> {
    let params = new HttpParams();
    
    if (filters) {
      if (filters.title) params = params.set('title', filters.title);
      if (filters.company) params = params.set('company', filters.company);
      if (filters.city) params = params.set('city', filters.city);
      if (filters.level) params = params.set('level', filters.level);
      if (filters.source) params = params.set('source', filters.source);
      if (filters.workType) params = params.set('workType', filters.workType);
      if (filters.page) params = params.set('pageNumber', filters.page.toString());
      if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    }

    return this.http.get<PagedJobsResponse>(`${this.apiUrl}/jobs/paged`, { params });
  }

  /**
   * Get job by ID
   */
  getJobById(id: string): Observable<Job> {
    return this.http.get<Job>(`${this.apiUrl}/jobs/${id}`);
  }

  /**
   * Search jobs by title and company
   */
  searchJobs(title?: string, company?: string): Observable<JobsResponse> {
    let params = new HttpParams();
    if (title) params = params.set('title', title);
    if (company) params = params.set('company', company);

    return this.http.get<JobsResponse>(`${this.apiUrl}/jobs/search`, { params });
  }

  /**
   * Get job statistics
   */
  getStats(): Observable<StatsResponse> {
    return this.http.get<StatsResponse>(`${this.apiUrl}/jobs/stats`);
  }

  /**
   * Fetch jobs from all sources
   */
  fetchJobs(options?: FetchOptions): Observable<FetchResult> {
    return this.http.post<FetchResult>(`${this.apiUrl}/fetch`, options || {});
  }

  /**
   * Fetch jobs from a specific source
   */
  fetchFromSource(source: string): Observable<FetchResult> {
    return this.http.post<FetchResult>(`${this.apiUrl}/fetch/${source}`, {});
  }

  /**
   * Get available sources
   */
  getSources(): Observable<{ sources: JobSource[] }> {
    return this.http.get<{ sources: JobSource[] }>(`${this.apiUrl}/fetch/sources`);
  }
}
