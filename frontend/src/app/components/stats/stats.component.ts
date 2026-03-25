import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobService } from '../../services/job.service';
import { StatsResponse } from '../../models/job.model';

@Component({
  selector: 'app-stats',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h1 class="page-title">
        <span class="icon">📊</span> Job Statistics
      </h1>

      <!-- Loading State -->
      <div *ngIf="loading()" class="loading-container">
        <div class="spinner"></div>
        <p>Loading statistics...</p>
      </div>

      <!-- Error State -->
      <div *ngIf="error()" class="error-message">
        <span class="icon">⚠️</span> {{ error() }}
      </div>

      <!-- Stats Content -->
      <div *ngIf="!loading() && stats()" class="stats-content">
        <!-- Total Jobs -->
        <div class="hero-stat">
          <span class="hero-value">{{ stats()!.totalJobs | number }}</span>
          <span class="hero-label">Total Jobs in Database</span>
        </div>

        <!-- Stats Grid -->
        <div class="stats-grid">
          <!-- By Level -->
          <div class="stats-card">
            <h2><span class="icon">📈</span> By Level</h2>
            <div class="stat-bars">
              <div *ngFor="let item of getStatItems(stats()!.byLevel)" class="stat-bar-item">
                <div class="stat-bar-header">
                  <span class="stat-bar-label">{{ item.key }}</span>
                  <span class="stat-bar-value">{{ item.value }}</span>
                </div>
                <div class="stat-bar-bg">
                  <div class="stat-bar-fill" [style.width.%]="getPercentage(item.value, stats()!.totalJobs)"></div>
                </div>
              </div>
            </div>
          </div>

          <!-- By Work Type -->
          <div class="stats-card">
            <h2><span class="icon">💼</span> By Work Type</h2>
            <div class="stat-bars">
              <div *ngFor="let item of getStatItems(stats()!.byWorkType)" class="stat-bar-item">
                <div class="stat-bar-header">
                  <span class="stat-bar-label">{{ item.key }}</span>
                  <span class="stat-bar-value">{{ item.value }}</span>
                </div>
                <div class="stat-bar-bg">
                  <div class="stat-bar-fill work-type" [style.width.%]="getPercentage(item.value, stats()!.totalJobs)"></div>
                </div>
              </div>
            </div>
          </div>

          <!-- By City -->
          <div class="stats-card">
            <h2><span class="icon">📍</span> By City</h2>
            <div class="stat-bars">
              <div *ngFor="let item of getStatItems(stats()!.byCity).slice(0, 8)" class="stat-bar-item">
                <div class="stat-bar-header">
                  <span class="stat-bar-label">{{ item.key || 'Not Specified' }}</span>
                  <span class="stat-bar-value">{{ item.value }}</span>
                </div>
                <div class="stat-bar-bg">
                  <div class="stat-bar-fill city" [style.width.%]="getPercentage(item.value, stats()!.totalJobs)"></div>
                </div>
              </div>
            </div>
          </div>

          <!-- By Source -->
          <div class="stats-card">
            <h2><span class="icon">🌐</span> By Source</h2>
            <div class="stat-bars">
              <div *ngFor="let item of getStatItems(stats()!.bySource)" class="stat-bar-item">
                <div class="stat-bar-header">
                  <span class="stat-bar-label">{{ item.key }}</span>
                  <span class="stat-bar-value">{{ item.value }}</span>
                </div>
                <div class="stat-bar-bg">
                  <div class="stat-bar-fill source" [style.width.%]="getPercentage(item.value, stats()!.totalJobs)"></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 2rem;
    }

    .page-title {
      color: var(--text-primary);
      font-size: 2rem;
      margin-bottom: 2rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
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

    .hero-stat {
      background: linear-gradient(135deg, var(--accent-primary) 0%, var(--accent-secondary) 100%);
      border-radius: 16px;
      padding: 3rem;
      text-align: center;
      margin-bottom: 2rem;
      border: none;
      box-shadow: 0 4px 20px var(--shadow-hover);
    }

    .hero-value {
      display: block;
      font-size: 4rem;
      font-weight: 700;
      color: #fff;
      line-height: 1;
      margin-bottom: 0.5rem;
    }

    .hero-label {
      color: rgba(255, 255, 255, 0.85);
      font-size: 1.25rem;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 1.5rem;
    }

    .stats-card {
      background: var(--bg-secondary);
      border-radius: 12px;
      padding: 1.5rem;
      border: 1px solid var(--border-color);
      box-shadow: 0 2px 8px var(--shadow-color);
    }

    .stats-card h2 {
      color: var(--text-primary);
      font-size: 1.1rem;
      margin: 0 0 1.5rem 0;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .stat-bars {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .stat-bar-item {
      width: 100%;
    }

    .stat-bar-header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 0.5rem;
    }

    .stat-bar-label {
      color: var(--text-secondary);
      font-size: 0.9rem;
    }

    .stat-bar-value {
      color: var(--accent-primary);
      font-weight: 600;
    }

    .stat-bar-bg {
      height: 8px;
      background: var(--bg-tertiary);
      border-radius: 4px;
      overflow: hidden;
    }

    .stat-bar-fill {
      height: 100%;
      background: linear-gradient(90deg, var(--accent-primary), var(--accent-secondary));
      border-radius: 4px;
      transition: width 0.5s ease;
    }

    .stat-bar-fill.work-type {
      background: linear-gradient(90deg, #9c27b0, #7b1fa2);
    }

    .stat-bar-fill.city {
      background: linear-gradient(90deg, #4caf50, #388e3c);
    }

    .stat-bar-fill.source {
      background: linear-gradient(90deg, #ff9800, #f57c00);
    }

    .icon {
      font-style: normal;
    }

    @media (max-width: 768px) {
      .stats-grid {
        grid-template-columns: 1fr;
      }

      .hero-value {
        font-size: 3rem;
      }
    }
  `]
})
export class StatsComponent implements OnInit {
  stats = signal<StatsResponse | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(private jobService: JobService) {}

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.loading.set(true);
    this.error.set(null);

    this.jobService.getStats().subscribe({
      next: (response) => {
        this.stats.set(response);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load statistics. Make sure the API is running.');
        this.loading.set(false);
        console.error('Error loading stats:', err);
      }
    });
  }

  getStatItems(obj: { [key: string]: number }): { key: string; value: number }[] {
    return Object.entries(obj)
      .map(([key, value]) => ({ key, value }))
      .sort((a, b) => b.value - a.value);
  }

  getPercentage(value: number, total: number): number {
    if (total === 0) return 0;
    return (value / total) * 100;
  }
}
