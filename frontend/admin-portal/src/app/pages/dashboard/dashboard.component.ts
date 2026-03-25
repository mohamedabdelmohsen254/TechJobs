import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { DashboardStats } from '../../models/job.model';

interface StatCard {
  title: string;
  value: number;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  stats = signal<DashboardStats | null>(null);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.apiService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Error loading dashboard stats. Make sure the API is running.');
        this.isLoading.set(false);
      }
    });
  }

  getStatCards(): StatCard[] {
    const s = this.stats();
    if (!s) return [];
    
    return [
      { title: 'Total Jobs', value: s.totalJobs, icon: '💼', color: 'bg-blue-500' },
      { title: 'Active Jobs', value: s.activeJobs, icon: '✅', color: 'bg-green-500' },
      { title: 'Visible to Users', value: s.visibleJobs, icon: '👁️', color: 'bg-emerald-500' },
      { title: 'Hidden from Users', value: s.hiddenJobs, icon: '🙈', color: 'bg-orange-500' },
      { title: 'Manual Entries', value: s.manualEntries, icon: '✍️', color: 'bg-purple-500' },
      { title: 'Added Today', value: s.jobsAddedToday, icon: '📅', color: 'bg-pink-500' },
    ];
  }

  getJobsByCountry(): [string, number][] {
    const s = this.stats();
    if (!s?.jobsByCountry) return [];
    return Object.entries(s.jobsByCountry).sort((a, b) => b[1] - a[1]);
  }

  getJobsByWorkType(): [string, number][] {
    const s = this.stats();
    if (!s?.jobsByWorkType) return [];
    return Object.entries(s.jobsByWorkType).sort((a, b) => b[1] - a[1]);
  }

  getJobsBySource(): [string, number][] {
    const s = this.stats();
    if (!s?.jobsBySource) return [];
    return Object.entries(s.jobsBySource).sort((a, b) => b[1] - a[1]);
  }
}
