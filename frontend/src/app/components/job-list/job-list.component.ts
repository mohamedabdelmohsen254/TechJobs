import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobService } from '../../services/job.service';
import { Job, FilterOptions } from '../../models/job.model';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <h1 class="page-title">
        <span class="icon">📋</span> Job Listings
        <span class="job-count" *ngIf="!loading()">{{ totalJobs() }} jobs found</span>
      </h1>

      <!-- Filters -->
      <div class="filters-card">
        <div class="filters-grid">
          <div class="filter-group">
            <label>Title</label>
            <input type="text" [(ngModel)]="filters.title" placeholder="Search by title..." 
                   (input)="onFilterChange()" class="filter-input">
          </div>
          <div class="filter-group">
            <label>Company</label>
            <input type="text" [(ngModel)]="filters.company" placeholder="Search by company..." 
                   (input)="onFilterChange()" class="filter-input">
          </div>
          <div class="filter-group">
            <label>City</label>
            <select [(ngModel)]="filters.city" (change)="onFilterChange()" class="filter-input">
              <option value="">All Cities</option>
              <option value="Cairo">Cairo</option>
              <option value="New Cairo">New Cairo</option>
              <option value="Giza">Giza</option>
              <option value="Alexandria">Alexandria</option>
              <option value="Smart Village">Smart Village</option>
            </select>
          </div>
          <div class="filter-group">
            <label>Level</label>
            <select [(ngModel)]="filters.level" (change)="onFilterChange()" class="filter-input">
              <option value="">All Levels</option>
              <option value="Intern">Intern</option>
              <option value="Junior">Junior</option>
              <option value="Mid">Mid</option>
              <option value="Senior">Senior</option>
              <option value="Lead">Lead</option>
              <option value="Principal">Principal</option>
            </select>
          </div>
          <div class="filter-group">
            <label>Work Type</label>
            <select [(ngModel)]="filters.workType" (change)="onFilterChange()" class="filter-input">
              <option value="">All Types</option>
              <option value="Remote">Remote</option>
              <option value="Hybrid">Hybrid</option>
              <option value="On-site">On-site</option>
            </select>
          </div>
          <div class="filter-group">
            <label>Source</label>
            <select [(ngModel)]="filters.source" (change)="onFilterChange()" class="filter-input">
              <option value="">All Sources</option>
              <option value="Greenhouse">Greenhouse</option>
              <option value="Lever">Lever</option>
              <option value="Workable">Workable</option>
              <option value="Jooble">Jooble</option>
              <option value="RemoteOK">RemoteOK</option>
              <option value="Remotive">Remotive</option>
              <option value="Himalayas">Himalayas</option>
              <option value="Jobicy">Jobicy</option>
            </select>
          </div>
          <div class="filter-group">
            <label>Sort By</label>
            <select [(ngModel)]="sortBy" (change)="onSortChange()" class="filter-input">
              <option value="date">Date</option>
              <option value="title">Title</option>
              <option value="company">Company</option>
              <option value="level">Level</option>
            </select>
          </div>
          <div class="filter-group">
            <label>Order</label>
            <select [(ngModel)]="sortOrder" (change)="onSortChange()" class="filter-input">
              <option value="desc">Newest First</option>
              <option value="asc">Oldest First</option>
            </select>
          </div>
        </div>
        <div class="filter-actions">
          <button (click)="clearFilters()" class="btn btn-secondary">
            <span class="icon">🗑️</span> Clear Filters
          </button>
          <button (click)="loadJobs()" class="btn btn-primary">
            <span class="icon">🔍</span> Search
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="loading()" class="loading-container">
        <div class="spinner"></div>
        <p>Loading jobs...</p>
      </div>

      <!-- Error State -->
      <div *ngIf="error()" class="error-message">
        <span class="icon">⚠️</span> {{ error() }}
      </div>

      <!-- Jobs Grid -->
      <div *ngIf="!loading() && !error()" class="jobs-grid">
        <div *ngFor="let job of jobs()" class="job-card">
          <div class="job-header">
            <h3 class="job-title">{{ job.title }}</h3>
            <span class="job-level" [class]="'level-' + job.level.toLowerCase()">{{ job.level }}</span>
          </div>
          <p class="job-company">
            <span class="icon">🏢</span> {{ job.company }}
          </p>
          <div class="job-meta">
            <span class="meta-item">
              <span class="icon">📍</span> {{ job.location || job.city || 'Not specified' }}
            </span>
            <span class="meta-item">
              <span class="icon">💼</span> {{ job.workType }}
            </span>
            <span class="meta-item" *ngIf="job.date">
              <span class="icon">📅</span> {{ formatDate(job.date) }}
            </span>
          </div>
          <div class="job-skills" *ngIf="job.skills">
            <span class="skill-tag" *ngFor="let skill of job.skills.split(',').slice(0, 3)">
              {{ skill.trim() }}
            </span>
          </div>
          <div class="job-footer">
            <span class="job-source">via {{ job.source }}</span>
            <a [href]="job.applyUrl" target="_blank" class="btn btn-apply">
              Apply <span class="icon">→</span>
            </a>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div *ngIf="!loading() && !error() && jobs().length === 0" class="empty-state">
        <span class="empty-icon">🔍</span>
        <h3>No jobs found</h3>
        <p>Try adjusting your filters or fetch new jobs</p>
      </div>

      <!-- Pagination -->
      <div *ngIf="!loading() && totalPages() > 1" class="pagination">
        <button (click)="goToPage(currentPage() - 1)" 
                [disabled]="currentPage() <= 1" 
                class="btn btn-page btn-nav">
          <span class="nav-arrow">‹</span>
        </button>
        
        <ng-container *ngFor="let page of getPageNumbers()">
          <span *ngIf="page === '...'" class="page-ellipsis">...</span>
          <button *ngIf="page !== '...'" 
                  (click)="goToPage(+page)" 
                  [class.active]="currentPage() === +page"
                  class="btn btn-page btn-number">
            {{ page }}
          </button>
        </ng-container>
        
        <button (click)="goToPage(currentPage() + 1)" 
                [disabled]="currentPage() >= totalPages()" 
                class="btn btn-page btn-nav">
          <span class="nav-arrow">›</span>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 0 2rem;
    }

    .page-title {
      color: var(--text-primary);
      font-size: 2rem;
      margin-bottom: 1.5rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .job-count {
      font-size: 1rem;
      color: var(--accent-primary);
      margin-left: auto;
      font-weight: normal;
    }

    .filters-card {
      background: var(--bg-secondary);
      border-radius: 12px;
      padding: 1.5rem;
      margin-bottom: 2rem;
      border: 1px solid var(--border-color);
      box-shadow: 0 2px 8px var(--shadow-color);
    }

    .filters-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .filter-group label {
      display: block;
      color: var(--text-secondary);
      font-size: 0.85rem;
      margin-bottom: 0.5rem;
    }

    .filter-input {
      width: 100%;
      padding: 0.75rem 1rem;
      border: 1px solid var(--border-color);
      border-radius: 8px;
      background: var(--bg-input);
      color: var(--text-primary);
      font-size: 0.95rem;
    }

    .filter-input:focus {
      outline: none;
      border-color: var(--accent-primary);
      background: var(--bg-secondary);
    }

    .filter-actions {
      display: flex;
      gap: 1rem;
      justify-content: flex-end;
    }

    .btn {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 8px;
      font-size: 0.95rem;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 0.5rem;
      transition: all 0.3s ease;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
      color: var(--accent-text);
      font-weight: 600;
    }

    .btn-primary:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 15px var(--shadow-hover);
    }

    .btn-secondary {
      background: var(--bg-tertiary);
      color: var(--text-primary);
    }

    .btn-secondary:hover {
      background: var(--border-color);
    }

    .jobs-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 1.5rem;
    }

    .job-card {
      background: var(--bg-secondary);
      border-radius: 12px;
      padding: 1.5rem;
      border: 1px solid var(--border-color);
      transition: all 0.3s ease;
      box-shadow: 0 2px 8px var(--shadow-color);
    }

    .job-card:hover {
      border-color: var(--accent-primary);
      transform: translateY(-4px);
      box-shadow: 0 8px 25px var(--shadow-hover);
    }

    .job-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 1rem;
      margin-bottom: 0.75rem;
    }

    .job-title {
      color: var(--text-primary);
      font-size: 1.1rem;
      margin: 0;
      flex: 1;
    }

    .job-level {
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
    }

    .level-senior { background: #4caf50; color: #fff; }
    .level-mid { background: #2196f3; color: #fff; }
    .level-junior { background: #ff9800; color: #fff; }
    .level-lead { background: #9c27b0; color: #fff; }
    .level-principal { background: #e91e63; color: #fff; }
    .level-intern { background: #607d8b; color: #fff; }

    .job-company {
      color: var(--accent-primary);
      margin: 0 0 0.75rem 0;
      font-size: 0.95rem;
    }

    .job-meta {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      margin-bottom: 1rem;
      color: var(--text-secondary);
      font-size: 0.85rem;
    }

    .meta-item {
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .job-skills {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-bottom: 1rem;
    }

    .skill-tag {
      background: rgba(0, 136, 204, 0.1);
      color: var(--accent-primary);
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.75rem;
    }

    .job-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: 1rem;
      border-top: 1px solid var(--border-color);
    }

    .job-source {
      color: var(--text-muted);
      font-size: 0.8rem;
    }

    .btn-apply {
      background: transparent;
      border: 1px solid var(--accent-primary);
      color: var(--accent-primary);
      padding: 0.5rem 1rem;
      text-decoration: none;
    }

    .btn-apply:hover {
      background: var(--accent-primary);
      color: var(--accent-text);
    }

    .loading-container {
      text-align: center;
      padding: 4rem;
      color: var(--text-secondary);
    }

    .spinner {
      width: 50px;
      height: 50px;
      border: 4px solid var(--border-color);
      border-top-color: var(--accent-primary);
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 1rem;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .error-message {
      background: rgba(244, 67, 54, 0.1);
      border: 1px solid #f44336;
      color: #d32f2f;
      padding: 1rem 1.5rem;
      border-radius: 8px;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .empty-state {
      text-align: center;
      padding: 4rem;
      color: var(--text-secondary);
    }

    .empty-icon {
      font-size: 4rem;
      display: block;
      margin-bottom: 1rem;
    }

    .empty-state h3 {
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 1rem;
      margin-top: 2rem;
      padding: 1rem;
    }

    .btn-page {
      background: transparent;
      color: var(--accent-primary);
      border: none;
      min-width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.95rem;
      border-radius: 50%;
      transition: all 0.2s ease;
    }

    .btn-page:hover:not(:disabled):not(.active) {
      background: rgba(0, 136, 204, 0.1);
    }

    .btn-page:disabled {
      opacity: 0.3;
      cursor: not-allowed;
      color: var(--text-muted);
    }

    .btn-page.active {
      background: var(--accent-primary);
      color: var(--accent-text);
      font-weight: 600;
    }

    .btn-nav {
      border: 1px solid var(--border-color);
    }

    .btn-nav:hover:not(:disabled) {
      border-color: var(--accent-primary);
    }

    .nav-arrow {
      font-size: 1.5rem;
      line-height: 1;
    }

    .btn-number {
      font-weight: 500;
    }

    .page-ellipsis {
      color: #666;
      padding: 0 0.5rem;
      display: flex;
      align-items: center;
    }

    .page-info {
      color: #888;
    }

    .icon {
      font-style: normal;
    }

    @media (max-width: 768px) {
      .filters-grid {
        grid-template-columns: 1fr;
      }

      .jobs-grid {
        grid-template-columns: 1fr;
      }

      .filter-actions {
        flex-direction: column;
      }
    }
  `]
})
export class JobListComponent implements OnInit {
  jobs = signal<Job[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  totalJobs = signal(0);
  currentPage = signal(1);
  pageSize = 20;
  totalPages = computed(() => Math.ceil(this.totalJobs() / this.pageSize));
  
  sortBy = 'date';
  sortOrder = 'desc';

  filters: FilterOptions = {
    title: '',
    company: '',
    city: '',
    level: '',
    source: '',
    workType: '',
    page: 1,
    pageSize: this.pageSize
  };

  private filterTimeout: any;

  constructor(private jobService: JobService) {}

  ngOnInit() {
    this.loadJobs();
  }

  loadJobs() {
    this.loading.set(true);
    this.error.set(null);

    this.filters.page = this.currentPage();
    this.filters.pageSize = this.pageSize;

    this.jobService.getPagedJobs(this.filters).subscribe({
      next: (response) => {
        let jobs = response.data.items;
        jobs = this.sortJobs(jobs);
        this.jobs.set(jobs);
        this.totalJobs.set(response.data.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load jobs. Make sure the API is running.');
        this.loading.set(false);
        console.error('Error loading jobs:', err);
      }
    });
  }

  sortJobs(jobs: Job[]): Job[] {
    return [...jobs].sort((a, b) => {
      let comparison = 0;
      switch (this.sortBy) {
        case 'date':
          const dateA = a.date ? new Date(a.date).getTime() : 0;
          const dateB = b.date ? new Date(b.date).getTime() : 0;
          comparison = dateA - dateB;
          break;
        case 'title':
          comparison = (a.title || '').localeCompare(b.title || '');
          break;
        case 'company':
          comparison = (a.company || '').localeCompare(b.company || '');
          break;
        case 'level':
          comparison = (a.level || '').localeCompare(b.level || '');
          break;
      }
      return this.sortOrder === 'desc' ? -comparison : comparison;
    });
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return 'Unknown';
    try {
      const date = new Date(dateStr);
      const now = new Date();
      const diffTime = now.getTime() - date.getTime();
      const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
      const formattedDate = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
      
      if (diffDays === 0) return 'Today';
      if (diffDays === 1) return `Yesterday (${formattedDate})`;
      if (diffDays < 7) return `${diffDays} days ago (${formattedDate})`;
      if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago (${formattedDate})`;
      if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago (${formattedDate})`;
      
      return formattedDate;
    } catch {
      return dateStr;
    }
  }

  onSortChange() {
    const currentJobs = this.jobs();
    this.jobs.set(this.sortJobs(currentJobs));
  }

  onFilterChange() {
    clearTimeout(this.filterTimeout);
    this.filterTimeout = setTimeout(() => {
      this.currentPage.set(1);
      this.loadJobs();
    }, 300);
  }

  clearFilters() {
    this.filters = {
      title: '',
      company: '',
      city: '',
      level: '',
      source: '',
      workType: '',
      page: 1,
      pageSize: this.pageSize
    };
    this.currentPage.set(1);
    this.loadJobs();
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadJobs();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  getPageNumbers(): (string | number)[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: (string | number)[] = [];
    
    if (total <= 10) {
      // Show all pages if 10 or less
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      // Always show first page
      pages.push(1);
      
      if (current > 4) {
        pages.push('...');
      }
      
      // Show pages around current
      const start = Math.max(2, current - 2);
      const end = Math.min(total - 1, current + 2);
      
      for (let i = start; i <= end; i++) {
        pages.push(i);
      }
      
      if (current < total - 3) {
        pages.push('...');
      }
      
      // Always show last page
      pages.push(total);
    }
    
    return pages;
  }
}
