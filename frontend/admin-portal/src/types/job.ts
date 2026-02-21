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
