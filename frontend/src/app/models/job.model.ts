export interface Job {
  jobId: string;
  title: string;
  company: string;
  level: string;
  salary: string;
  experienceYears: string;
  skills: string;
  source: string;
  sourceId: string;
  sourceType: string;
  allowedMode: string;
  attributionRequired: string;
  sourceUrl: string;
  rateLimitRpm: number;
  rateLimitBurst: number;
  takedownContact: string;
  termsUrl: string;
  sourceNotes: string;
  country: string;
  city: string;
  workType: string;
  location: string;
  applyUrl: string;
  date: string | null;
}

export interface JobsResponse {
  success: boolean;
  message: string;
  data: Job[];
}

export interface PagedJobsResponse {
  success: boolean;
  message: string;
  data: {
    items: Job[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  };
}

export interface FetchOptions {
  fetchGreenhouse: boolean;
  fetchLever: boolean;
  fetchWorkable: boolean;
  fetchJooble: boolean;
  fetchRemoteOk: boolean;
  fetchRemotive: boolean;
  fetchHimalayas: boolean;
  fetchJobicy: boolean;
  joobleMaxPages: number;
}

export interface FetchResult {
  success: boolean;
  message: string;
  startTime: string;
  endTime: string;
  durationSeconds: number;
  totalFetched: number;
  afterDeduplication: number;
  savedToCsv: number;
  sourceStats: { [key: string]: number };
}

export interface JobSource {
  id: string;
  name: string;
  description: string;
  rateLimit: string;
}

export interface FilterOptions {
  title?: string;
  company?: string;
  city?: string;
  level?: string;
  source?: string;
  workType?: string;
  page?: number;
  pageSize?: number;
}

export interface StatsResponse {
  totalJobs: number;
  byCity: { [key: string]: number };
  byLevel: { [key: string]: number };
  bySource: { [key: string]: number };
  byWorkType: { [key: string]: number };
}
