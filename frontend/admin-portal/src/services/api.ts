import axios from 'axios';
import { Job, CreateJobDto, UpdateJobDto, PaginatedResponse, DashboardStats, BlockedCompany, BlockedKeyword, CreateBlockedCompanyDto, CreateBlockedKeywordDto, FetchOptions, FetchAndSyncResult } from '../types/job';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5100';

const axiosInstance = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
axiosInstance.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 responses
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export interface GetJobsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  country?: string;
  workType?: string;
  isActive?: boolean;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: {
    id: number;
    username: string;
    email: string;
    fullName: string;
  };
}

export const api = {
  login: async (email: string, password: string): Promise<LoginResponse> => {
    const { data } = await axiosInstance.post('/auth/login', { email, password });
    return data;
  },

  getCurrentUser: async () => {
    const { data } = await axiosInstance.get('/auth/me');
    return data;
  },
};

export const jobsApi = {
  getJobs: async (params: GetJobsParams = {}): Promise<PaginatedResponse<Job>> => {
    const { data } = await axiosInstance.get('/jobs', { params });
    return data;
  },

  getJob: async (id: number): Promise<Job> => {
    const { data } = await axiosInstance.get(`/jobs/${id}`);
    return data;
  },

  createJob: async (job: CreateJobDto): Promise<Job> => {
    const { data } = await axiosInstance.post('/jobs', job);
    return data;
  },

  updateJob: async (id: number, job: UpdateJobDto): Promise<Job> => {
    const { data } = await axiosInstance.put(`/jobs/${id}`, job);
    return data;
  },

  deleteJob: async (id: number): Promise<void> => {
    await axiosInstance.delete(`/jobs/${id}`);
  },

  bulkCreateJobs: async (jobs: CreateJobDto[]): Promise<{ created: number; jobs: Job[] }> => {
    const { data } = await axiosInstance.post('/jobs/bulk', jobs);
    return data;
  },

  toggleVisibility: async (id: number, visible: boolean): Promise<Job> => {
    const { data } = await axiosInstance.patch(`/jobs/${id}/visibility?visible=${visible}`);
    return data;
  },

  bulkToggleVisibility: async (jobIds: number[], visible: boolean): Promise<{ message: string; updated: number }> => {
    const { data } = await axiosInstance.patch('/jobs/bulk-visibility', { jobIds, visible });
    return data;
  },
};

export const dashboardApi = {
  getStats: async (): Promise<DashboardStats> => {
    const { data } = await axiosInstance.get('/dashboard/stats');
    return data;
  },
};

export const syncApi = {
  syncFromCsvApi: async (apiUrl: string = 'http://localhost:5200'): Promise<any> => {
    const { data } = await axiosInstance.post(
      `/sync/from-csv-api?apiUrl=${encodeURIComponent(apiUrl)}`,
      undefined,
      { timeout: 120000 }
    );
    return data;
  },

  fetchAndSync: async (apiUrl: string = 'http://localhost:5200', options?: FetchOptions): Promise<FetchAndSyncResult> => {
    const { data } = await axiosInstance.post(
      `/sync/fetch-and-sync?apiUrl=${encodeURIComponent(apiUrl)}`,
      options || {},
      { timeout: 600000 } // 10 minute timeout for fetch operations
    );
    return data;
  },

  getStatus: async (): Promise<{ totalJobs: number; importedJobs: number; manualJobs: number; lastSyncAt?: string }> => {
    const { data } = await axiosInstance.get('/sync/status');
    return data;
  },
};

export const filtersApi = {
  // Blocked Companies
  getBlockedCompanies: async (isActive?: boolean): Promise<BlockedCompany[]> => {
    const params = isActive !== undefined ? { isActive } : {};
    const { data } = await axiosInstance.get('/filters/companies', { params });
    return data;
  },

  addBlockedCompany: async (dto: CreateBlockedCompanyDto): Promise<BlockedCompany> => {
    const { data } = await axiosInstance.post('/filters/companies', dto);
    return data;
  },

  toggleBlockedCompany: async (id: number): Promise<BlockedCompany> => {
    const { data } = await axiosInstance.patch(`/filters/companies/${id}/toggle`);
    return data;
  },

  deleteBlockedCompany: async (id: number): Promise<void> => {
    await axiosInstance.delete(`/filters/companies/${id}`);
  },

  getCompanySuggestions: async (search?: string): Promise<string[]> => {
    const params = search ? { search } : {};
    const { data } = await axiosInstance.get('/filters/companies/suggestions', { params });
    return data;
  },

  // Blocked Keywords
  getBlockedKeywords: async (isActive?: boolean): Promise<BlockedKeyword[]> => {
    const params = isActive !== undefined ? { isActive } : {};
    const { data } = await axiosInstance.get('/filters/keywords', { params });
    return data;
  },

  addBlockedKeyword: async (dto: CreateBlockedKeywordDto): Promise<BlockedKeyword> => {
    const { data } = await axiosInstance.post('/filters/keywords', dto);
    return data;
  },

  toggleBlockedKeyword: async (id: number): Promise<BlockedKeyword> => {
    const { data } = await axiosInstance.patch(`/filters/keywords/${id}/toggle`);
    return data;
  },

  deleteBlockedKeyword: async (id: number): Promise<void> => {
    await axiosInstance.delete(`/filters/keywords/${id}`);
  },

  bulkAddKeywords: async (keywords: CreateBlockedKeywordDto[]): Promise<{ added: number }> => {
    const { data } = await axiosInstance.post('/filters/keywords/bulk', keywords);
    return data;
  },
};

export default axiosInstance;
