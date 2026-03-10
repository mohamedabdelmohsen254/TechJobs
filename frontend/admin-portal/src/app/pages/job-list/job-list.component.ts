import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Job, PaginatedResponse, GetJobsParams } from '../../models/job.model';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './job-list.component.html'
})
export class JobListComponent implements OnInit {
  jobs = signal<PaginatedResponse<Job> | null>(null);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);
  selectedJobs = signal<number[]>([]);
  
  params: GetJobsParams = {
    page: 1,
    pageSize: 20,
    search: '',
    isActive: undefined
  };

  searchInput = '';
  statusFilter = '';

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadJobs();
  }

  loadJobs(): void {
    this.isLoading.set(true);
    this.apiService.getJobs(this.params).subscribe({
      next: (data) => {
        this.jobs.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Error loading jobs. Make sure the API is running.');
        this.isLoading.set(false);
      }
    });
  }

  onSearch(): void {
    this.params.search = this.searchInput;
    this.params.page = 1;
    this.loadJobs();
  }

  onStatusChange(): void {
    this.params.isActive = this.statusFilter === '' ? undefined : this.statusFilter === 'true';
    this.params.page = 1;
    this.loadJobs();
  }

  handleDelete(id: number, title: string): void {
    if (confirm(`Are you sure you want to delete "${title}"?`)) {
      this.apiService.deleteJob(id).subscribe({
        next: () => this.loadJobs(),
        error: (err) => alert('Failed to delete job')
      });
    }
  }

  handleToggleVisibility(id: number, currentVisible: boolean): void {
    this.apiService.toggleVisibility(id, !currentVisible).subscribe({
      next: () => this.loadJobs(),
      error: (err) => alert('Failed to toggle visibility')
    });
  }

  handleBulkVisibility(visible: boolean): void {
    const selected = this.selectedJobs();
    if (selected.length === 0) {
      alert('Please select jobs first');
      return;
    }
    this.apiService.bulkToggleVisibility(selected, visible).subscribe({
      next: () => {
        this.loadJobs();
        this.selectedJobs.set([]);
      },
      error: (err) => alert('Failed to update visibility')
    });
  }

  handleSelectAll(): void {
    const data = this.jobs();
    if (data?.items) {
      if (this.selectedJobs().length === data.items.length) {
        this.selectedJobs.set([]);
      } else {
        this.selectedJobs.set(data.items.map(j => j.id));
      }
    }
  }

  handleSelectJob(id: number): void {
    const current = this.selectedJobs();
    if (current.includes(id)) {
      this.selectedJobs.set(current.filter(j => j !== id));
    } else {
      this.selectedJobs.set([...current, id]);
    }
  }

  isSelected(id: number): boolean {
    return this.selectedJobs().includes(id);
  }

  isAllSelected(): boolean {
    const data = this.jobs();
    return data?.items ? this.selectedJobs().length === data.items.length && data.items.length > 0 : false;
  }

  clearSelection(): void {
    this.selectedJobs.set([]);
  }

  goToPage(page: number): void {
    this.params.page = page;
    this.loadJobs();
  }

  getLocationDisplay(job: Job): string {
    if (job.city && job.country) {
      return `${job.city}, ${job.country}`;
    }
    return job.country || job.location || '-';
  }

  getShowingRange(): string {
    const data = this.jobs();
    if (!data) return '';
    const start = ((data.page - 1) * data.pageSize) + 1;
    const end = Math.min(data.page * data.pageSize, data.totalCount);
    return `Showing ${start} to ${end} of ${data.totalCount} results`;
  }
}
