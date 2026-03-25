import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Job,
  CreateJobDto,
  UpdateJobDto,
  PaginatedResponse,
  DashboardStats,
  BlockedCompany,
  BlockedKeyword,
  CreateBlockedCompanyDto,
  CreateBlockedKeywordDto,
  FetchOptions,
  FetchAndSyncResult,
  GetJobsParams
} from '../models/job.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly API_URL = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) {}

  // Jobs API
  getJobs(params: GetJobsParams = {}): Observable<PaginatedResponse<Job>> {
    let httpParams = new HttpParams();
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.country) httpParams = httpParams.set('country', params.country);
    if (params.workType) httpParams = httpParams.set('workType', params.workType);
    if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    
    return this.http.get<PaginatedResponse<Job>>(`${this.API_URL}/jobs`, { params: httpParams });
  }

  getJob(id: number): Observable<Job> {
    return this.http.get<Job>(`${this.API_URL}/jobs/${id}`);
  }

  createJob(job: CreateJobDto): Observable<Job> {
    return this.http.post<Job>(`${this.API_URL}/jobs`, job);
  }

  updateJob(id: number, job: UpdateJobDto): Observable<Job> {
    return this.http.put<Job>(`${this.API_URL}/jobs/${id}`, job);
  }

  deleteJob(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/jobs/${id}`);
  }

  bulkCreateJobs(jobs: CreateJobDto[]): Observable<{ created: number; jobs: Job[] }> {
    return this.http.post<{ created: number; jobs: Job[] }>(`${this.API_URL}/jobs/bulk`, jobs);
  }

  toggleVisibility(id: number, visible: boolean): Observable<Job> {
    return this.http.patch<Job>(`${this.API_URL}/jobs/${id}/visibility?visible=${visible}`, {});
  }

  bulkToggleVisibility(jobIds: number[], visible: boolean): Observable<{ message: string; updated: number }> {
    return this.http.patch<{ message: string; updated: number }>(`${this.API_URL}/jobs/bulk-visibility`, { jobIds, visible });
  }

  // Dashboard API
  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.API_URL}/dashboard/stats`);
  }

  // Sync API
  syncFromCsvApi(apiUrl: string = 'http://localhost:5200'): Observable<any> {
    return this.http.post(`${this.API_URL}/sync/from-csv-api?apiUrl=${encodeURIComponent(apiUrl)}`, undefined);
  }

  fetchAndSync(apiUrl: string = 'http://localhost:5200', options?: FetchOptions): Observable<FetchAndSyncResult> {
    return this.http.post<FetchAndSyncResult>(
      `${this.API_URL}/sync/fetch-and-sync?apiUrl=${encodeURIComponent(apiUrl)}`,
      options || {}
    );
  }

  getSyncStatus(): Observable<{ totalJobs: number; importedJobs: number; manualJobs: number; lastSyncAt?: string }> {
    return this.http.get<{ totalJobs: number; importedJobs: number; manualJobs: number; lastSyncAt?: string }>(`${this.API_URL}/sync/status`);
  }

  // Filters API - Blocked Companies
  getBlockedCompanies(isActive?: boolean): Observable<BlockedCompany[]> {
    let params = new HttpParams();
    if (isActive !== undefined) params = params.set('isActive', isActive.toString());
    return this.http.get<BlockedCompany[]>(`${this.API_URL}/filters/companies`, { params });
  }

  addBlockedCompany(dto: CreateBlockedCompanyDto): Observable<BlockedCompany> {
    return this.http.post<BlockedCompany>(`${this.API_URL}/filters/companies`, dto);
  }

  toggleBlockedCompany(id: number): Observable<BlockedCompany> {
    return this.http.patch<BlockedCompany>(`${this.API_URL}/filters/companies/${id}/toggle`, {});
  }

  deleteBlockedCompany(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/filters/companies/${id}`);
  }

  getCompanySuggestions(search?: string): Observable<string[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<string[]>(`${this.API_URL}/filters/companies/suggestions`, { params });
  }

  // Filters API - Blocked Keywords
  getBlockedKeywords(isActive?: boolean): Observable<BlockedKeyword[]> {
    let params = new HttpParams();
    if (isActive !== undefined) params = params.set('isActive', isActive.toString());
    return this.http.get<BlockedKeyword[]>(`${this.API_URL}/filters/keywords`, { params });
  }

  addBlockedKeyword(dto: CreateBlockedKeywordDto): Observable<BlockedKeyword> {
    return this.http.post<BlockedKeyword>(`${this.API_URL}/filters/keywords`, dto);
  }

  toggleBlockedKeyword(id: number): Observable<BlockedKeyword> {
    return this.http.patch<BlockedKeyword>(`${this.API_URL}/filters/keywords/${id}/toggle`, {});
  }

  deleteBlockedKeyword(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/filters/keywords/${id}`);
  }

  bulkAddKeywords(keywords: CreateBlockedKeywordDto[]): Observable<{ added: number }> {
    return this.http.post<{ added: number }>(`${this.API_URL}/filters/keywords/bulk`, keywords);
  }
}
