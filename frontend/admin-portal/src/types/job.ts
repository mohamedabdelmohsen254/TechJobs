export interface Job {
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

export interface CreateJobDto {
  title: string;
  company: string;
  location?: string;
  country?: string;
  city?: string;
  workType?: string;
  applyUrl: string;
  source?: string;
  postedDate?: string;
  description?: string;
  salaryRange?: string;
  tags?: string;
  isVisibleToUsers?: boolean;
}

export interface UpdateJobDto extends Partial<CreateJobDto> {
  isActive?: boolean;
  isVisibleToUsers?: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DashboardStats {
  totalJobs: number;
  activeJobs: number;
  visibleJobs: number;
  hiddenJobs: number;
  manualEntries: number;
  jobsAddedToday: number;
  jobsByCountry: Record<string, number>;
  jobsByWorkType: Record<string, number>;
  jobsBySource: Record<string, number>;
}

// Filter types
export interface BlockedCompany {
  id: number;
  companyName: string;
  reason?: string;
  createdAt: string;
  isActive: boolean;
}

export interface BlockedKeyword {
  id: number;
  keyword: string;
  reason?: string;
  createdAt: string;
  isActive: boolean;
}

export interface CreateBlockedCompanyDto {
  companyName: string;
  reason?: string;
}

export interface CreateBlockedKeywordDto {
  keyword: string;
  reason?: string;
}

// Fetch types
export interface FetchOptions {
  fetchGreenhouse?: boolean;
  fetchLever?: boolean;
  fetchWorkable?: boolean;
  fetchJooble?: boolean;
  fetchRemoteOk?: boolean;
  fetchRemotive?: boolean;
  fetchHimalayas?: boolean;
  fetchJobicy?: boolean;
  joobleMaxPages?: number;
}

export interface FetchAndSyncResult {
  message: string;
  fetchResult: {
    totalFetched: number;
    afterDeduplication: number;
    savedToMainApi: number;
    durationSeconds: number;
    sourceStats?: Record<string, number>;
  };
  syncResult: {
    imported: number;
    skipped: number;
    total: number;
  };
}
