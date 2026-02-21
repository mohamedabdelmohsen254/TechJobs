import axios from 'axios';
import { Job, CreateJobDto, UpdateJobDto, PaginatedResponse, DashboardStats } from '../types/job';

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
    const { data } = await axiosInstance.post(`/sync/from-csv-api?apiUrl=${encodeURIComponent(apiUrl)}`);
    return data;
  },
};

export default axiosInstance;
