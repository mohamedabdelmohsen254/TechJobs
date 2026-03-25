import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { JobService } from '../../services/job.service';
import { FetchOptions, FetchResult, JobSource } from '../../models/job.model';

@Component({
  selector: 'app-fetch-jobs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <h1 class="page-title">
        <span class="icon">🔄</span> Fetch Jobs
      </h1>
      <p class="page-description">
        Fetch new job listings from various sources and save them to the database.
      </p>

      <!-- Source Selection -->
      <div class="sources-card">
        <h2>Select Sources</h2>
        <div class="sources-grid">
          <label *ngFor="let source of sources()" class="source-checkbox">
            <input type="checkbox" [(ngModel)]="sourceSelection[source.id]">
            <div class="source-info">
              <span class="source-name">{{ source.name }}</span>
              <span class="source-desc">{{ source.description }}</span>
              <span class="source-rate">{{ source.rateLimit }}</span>
            </div>
          </label>
        </div>
        
        <div class="jooble-pages" *ngIf="sourceSelection['jooble']">
          <label>Jooble Max Pages</label>
          <input type="number" [(ngModel)]="joobleMaxPages" min="1" max="10" class="page-input">
        </div>

        <div class="fetch-actions">
          <button (click)="selectAll()" class="btn btn-secondary">
            <span class="icon">✅</span> Select All
          </button>
          <button (click)="deselectAll()" class="btn btn-secondary">
            <span class="icon">❌</span> Deselect All
          </button>
          <button (click)="fetchJobs()" [disabled]="fetching()" class="btn btn-primary">
            <span *ngIf="!fetching()" class="icon">🚀</span>
            <span *ngIf="fetching()" class="spinner-small"></span>
            {{ fetching() ? 'Fetching...' : 'Fetch Jobs' }}
          </button>
        </div>
      </div>

      <!-- Progress -->
      <div *ngIf="fetching()" class="progress-card">
        <div class="progress-content">
          <div class="spinner-large"></div>
          <h3>Fetching jobs from selected sources...</h3>
          <p>This may take a few minutes depending on the number of sources selected.</p>
        </div>
      </div>

      <!-- Results -->
      <div *ngIf="lastResult()" class="results-card" [class.success]="lastResult()!.success">
        <h2>
          <span class="icon">{{ lastResult()!.success ? '✅' : '❌' }}</span>
          {{ lastResult()!.success ? 'Fetch Completed' : 'Fetch Failed' }}
        </h2>
        
        <div *ngIf="lastResult()!.success" class="result-stats">
          <div class="stat-card">
            <span class="stat-value">{{ lastResult()!.totalFetched }}</span>
            <span class="stat-label">Total Fetched</span>
          </div>
          <div class="stat-card">
            <span class="stat-value">{{ lastResult()!.afterDeduplication }}</span>
            <span class="stat-label">After Dedup</span>
          </div>
          <div class="stat-card">
            <span class="stat-value">{{ lastResult()!.savedToCsv }}</span>
            <span class="stat-label">Saved to CSV</span>
          </div>
          <div class="stat-card">
            <span class="stat-value">{{ lastResult()!.durationSeconds.toFixed(1) }}s</span>
            <span class="stat-label">Duration</span>
          </div>
        </div>

        <div *ngIf="lastResult()!.sourceStats" class="source-stats">
          <h3>Jobs by Source</h3>
          <div class="source-stat-grid">
            <div *ngFor="let stat of getSourceStats()" class="source-stat-item">
              <span class="source-stat-name">{{ stat.source }}</span>
              <span class="source-stat-value">{{ stat.count }}</span>
            </div>
          </div>
        </div>

        <p *ngIf="!lastResult()!.success" class="error-text">
          {{ lastResult()!.message }}
        </p>
      </div>

      <!-- History -->
      <div *ngIf="fetchHistory().length > 0" class="history-card">
        <h2><span class="icon">📜</span> Fetch History</h2>
        <div class="history-list">
          <div *ngFor="let item of fetchHistory()" class="history-item">
            <span class="history-time">{{ formatDate(item.endTime) }}</span>
            <span class="history-count">{{ item.savedToCsv }} jobs</span>
            <span class="history-duration">{{ item.durationSeconds.toFixed(1) }}s</span>
            <span class="history-status" [class.success]="item.success">
              {{ item.success ? '✅' : '❌' }}
            </span>
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
      margin-bottom: 0.5rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .page-description {
      color: var(--text-secondary);
      margin-bottom: 2rem;
    }

    .sources-card, .progress-card, .results-card, .history-card {
      background: var(--bg-secondary);
      border-radius: 12px;
      padding: 1.5rem;
      margin-bottom: 1.5rem;
      border: 1px solid var(--border-color);
      box-shadow: 0 2px 8px var(--shadow-color);
    }

    .sources-card h2, .results-card h2, .history-card h2 {
      color: var(--text-primary);
      margin: 0 0 1rem 0;
      font-size: 1.25rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .sources-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1rem;
      margin-bottom: 1.5rem;
    }

    .source-checkbox {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      padding: 1rem;
      background: var(--bg-tertiary);
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.3s ease;
      border: 1px solid transparent;
    }

    .source-checkbox:hover {
      border-color: var(--accent-primary);
    }

    .source-checkbox input {
      width: 20px;
      height: 20px;
      margin-top: 2px;
      accent-color: var(--accent-primary);
    }

    .source-info {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .source-name {
      color: var(--text-primary);
      font-weight: 600;
    }

    .source-desc {
      color: var(--text-secondary);
      font-size: 0.85rem;
    }

    .source-rate {
      color: var(--accent-primary);
      font-size: 0.75rem;
    }

    .jooble-pages {
      margin-bottom: 1.5rem;
    }

    .jooble-pages label {
      display: block;
      color: var(--text-secondary);
      margin-bottom: 0.5rem;
    }

    .page-input {
      width: 100px;
      padding: 0.5rem 1rem;
      border: 1px solid var(--border-color);
      border-radius: 8px;
      background: var(--bg-tertiary);
      color: var(--text-primary);
    }

    .fetch-actions {
      display: flex;
      gap: 1rem;
      flex-wrap: wrap;
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

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 4px 15px var(--shadow-hover);
    }

    .btn-primary:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    .btn-secondary {
      background: var(--bg-tertiary);
      color: var(--text-primary);
    }

    .btn-secondary:hover {
      background: var(--border-color);
    }

    .spinner-small {
      width: 16px;
      height: 16px;
      border: 2px solid rgba(255, 255, 255, 0.2);
      border-top-color: var(--accent-text);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }

    .spinner-large {
      width: 60px;
      height: 60px;
      border: 4px solid var(--border-color);
      border-top-color: var(--accent-primary);
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 1rem;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .progress-card {
      text-align: center;
      padding: 3rem;
    }

    .progress-card h3 {
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .progress-card p {
      color: var(--text-secondary);
    }

    .results-card.success {
      border-color: #4caf50;
    }

    .result-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 1rem;
      margin-bottom: 1.5rem;
    }

    .stat-card {
      background: var(--bg-tertiary);
      padding: 1.5rem;
      border-radius: 8px;
      text-align: center;
    }

    .stat-value {
      display: block;
      font-size: 2rem;
      font-weight: 700;
      color: var(--accent-primary);
    }

    .stat-label {
      color: var(--text-secondary);
      font-size: 0.85rem;
    }

    .source-stats h3 {
      color: var(--text-primary);
      font-size: 1rem;
      margin-bottom: 1rem;
    }

    .source-stat-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
      gap: 0.5rem;
    }

    .source-stat-item {
      display: flex;
      justify-content: space-between;
      padding: 0.5rem 1rem;
      background: var(--bg-tertiary);
      border-radius: 6px;
    }

    .source-stat-name {
      color: var(--text-secondary);
    }

    .source-stat-value {
      color: var(--accent-primary);
      font-weight: 600;
    }

    .error-text {
      color: #d32f2f;
    }

    .history-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .history-item {
      display: flex;
      gap: 1rem;
      padding: 0.75rem 1rem;
      background: var(--bg-tertiary);
      border-radius: 6px;
      align-items: center;
    }

    .history-time {
      color: var(--text-secondary);
      flex: 1;
    }

    .history-count {
      color: var(--accent-primary);
    }

    .history-duration {
      color: var(--text-secondary);
    }

    .icon {
      font-style: normal;
    }

    @media (max-width: 768px) {
      .sources-grid {
        grid-template-columns: 1fr;
      }

      .fetch-actions {
        flex-direction: column;
      }
    }
  `]
})
export class FetchJobsComponent implements OnInit {
  sources = signal<JobSource[]>([]);
  fetching = signal(false);
  lastResult = signal<FetchResult | null>(null);
  fetchHistory = signal<FetchResult[]>([]);
  
  sourceSelection: { [key: string]: boolean } = {};
  joobleMaxPages = 3;

  constructor(private jobService: JobService) {}

  ngOnInit() {
    this.loadSources();
  }

  loadSources() {
    this.jobService.getSources().subscribe({
      next: (response) => {
        this.sources.set(response.sources);
        // Initialize all sources as selected
        response.sources.forEach(s => this.sourceSelection[s.id] = true);
      },
      error: (err) => {
        console.error('Error loading sources:', err);
      }
    });
  }

  selectAll() {
    this.sources().forEach(s => this.sourceSelection[s.id] = true);
  }

  deselectAll() {
    this.sources().forEach(s => this.sourceSelection[s.id] = false);
  }

  fetchJobs() {
    const options: FetchOptions = {
      fetchGreenhouse: this.sourceSelection['greenhouse'] || false,
      fetchLever: this.sourceSelection['lever'] || false,
      fetchWorkable: this.sourceSelection['workable'] || false,
      fetchJooble: this.sourceSelection['jooble'] || false,
      fetchRemoteOk: this.sourceSelection['remoteok'] || false,
      fetchRemotive: this.sourceSelection['remotive'] || false,
      fetchHimalayas: this.sourceSelection['himalayas'] || false,
      fetchJobicy: this.sourceSelection['jobicy'] || false,
      joobleMaxPages: this.joobleMaxPages
    };

    this.fetching.set(true);
    this.jobService.fetchJobs(options).subscribe({
      next: (result) => {
        this.lastResult.set(result);
        this.fetchHistory.update(history => [result, ...history.slice(0, 9)]);
        this.fetching.set(false);
      },
      error: (err) => {
        this.lastResult.set({
          success: false,
          message: 'Failed to fetch jobs. Make sure the API is running.',
          startTime: new Date().toISOString(),
          endTime: new Date().toISOString(),
          durationSeconds: 0,
          totalFetched: 0,
          afterDeduplication: 0,
          savedToCsv: 0,
          sourceStats: {}
        });
        this.fetching.set(false);
        console.error('Error fetching jobs:', err);
      }
    });
  }

  getSourceStats(): { source: string; count: number }[] {
    const stats = this.lastResult()?.sourceStats || {};
    return Object.entries(stats).map(([source, count]) => ({ source, count }));
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleString();
  }
}
