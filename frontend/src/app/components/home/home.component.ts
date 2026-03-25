import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container">
      <section class="hero">
        <h1 class="hero-title">
          Find Your Dream <span class="highlight">Tech Job</span> in Egypt
        </h1>
        <p class="hero-subtitle">
          Aggregating job listings from Greenhouse, Lever, Workable, Jooble, RemoteOK, and more.
        </p>
        <div class="hero-actions">
          <a routerLink="/jobs" class="btn btn-primary">
            <span class="icon">🔍</span> Browse Jobs
          </a>
          <a routerLink="/fetch" class="btn btn-secondary">
            <span class="icon">🔄</span> Fetch New Jobs
          </a>
        </div>
      </section>

      <section class="features">
        <div class="feature-card">
          <span class="feature-icon">🌐</span>
          <h3>Multiple Sources</h3>
          <p>Jobs aggregated from 8+ different platforms including Greenhouse, Lever, Workable, and more.</p>
        </div>
        <div class="feature-card">
          <span class="feature-icon">🔍</span>
          <h3>Smart Filtering</h3>
          <p>Filter by title, company, city, experience level, work type, and source.</p>
        </div>
        <div class="feature-card">
          <span class="feature-icon">🚀</span>
          <h3>Real-time Fetching</h3>
          <p>Fetch fresh job listings from all sources with a single click.</p>
        </div>
        <div class="feature-card">
          <span class="feature-icon">📊</span>
          <h3>Analytics</h3>
          <p>View statistics about job distribution by level, city, work type, and source.</p>
        </div>
      </section>

      <section class="sources">
        <h2>Powered By</h2>
        <div class="source-logos">
          <div class="source-item">Greenhouse</div>
          <div class="source-item">Lever</div>
          <div class="source-item">Workable</div>
          <div class="source-item">Jooble</div>
          <div class="source-item">RemoteOK</div>
          <div class="source-item">Remotive</div>
          <div class="source-item">Himalayas</div>
          <div class="source-item">Jobicy</div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 2rem;
    }

    .hero {
      text-align: center;
      padding: 4rem 0;
    }

    .hero-title {
      font-size: 3rem;
      color: var(--text-primary);
      margin-bottom: 1rem;
      line-height: 1.2;
    }

    .highlight {
      color: var(--accent-primary);
    }

    .hero-subtitle {
      font-size: 1.25rem;
      color: var(--text-secondary);
      margin-bottom: 2rem;
      max-width: 600px;
      margin-left: auto;
      margin-right: auto;
    }

    .hero-actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    .btn {
      padding: 1rem 2rem;
      border: none;
      border-radius: 8px;
      font-size: 1rem;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
      transition: all 0.3s ease;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--accent-primary), var(--accent-secondary));
      color: var(--accent-text);
      font-weight: 600;
    }

    .btn-primary:hover {
      transform: translateY(-3px);
      box-shadow: 0 8px 25px var(--shadow-hover);
    }

    .btn-secondary {
      background: var(--bg-secondary);
      color: var(--text-primary);
      border: 1px solid var(--border-color);
    }

    .btn-secondary:hover {
      border-color: var(--accent-primary);
      color: var(--accent-primary);
    }

    .features {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1.5rem;
      padding: 2rem 0 4rem;
    }

    .feature-card {
      background: var(--bg-secondary);
      border-radius: 12px;
      padding: 2rem;
      text-align: center;
      border: 1px solid var(--border-color);
      transition: all 0.3s ease;
      box-shadow: 0 2px 8px var(--shadow-color);
    }

    .feature-card:hover {
      border-color: var(--accent-primary);
      transform: translateY(-5px);
      box-shadow: 0 8px 25px var(--shadow-hover);
    }

    .feature-icon {
      font-size: 3rem;
      display: block;
      margin-bottom: 1rem;
    }

    .feature-card h3 {
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .feature-card p {
      color: var(--text-secondary);
      font-size: 0.95rem;
      line-height: 1.6;
    }

    .sources {
      text-align: center;
      padding: 3rem 0;
      border-top: 1px solid var(--border-color);
    }

    .sources h2 {
      color: var(--text-muted);
      font-size: 1rem;
      text-transform: uppercase;
      letter-spacing: 2px;
      margin-bottom: 2rem;
    }

    .source-logos {
      display: flex;
      flex-wrap: wrap;
      justify-content: center;
      gap: 1rem;
    }

    .source-item {
      background: var(--bg-secondary);
      padding: 0.75rem 1.5rem;
      border-radius: 8px;
      color: var(--text-secondary);
      font-size: 0.9rem;
      border: 1px solid var(--border-color);
    }

    .icon {
      font-style: normal;
    }

    @media (max-width: 768px) {
      .hero-title {
        font-size: 2rem;
      }

      .hero-subtitle {
        font-size: 1rem;
      }
    }
  `]
})
export class HomeComponent {}
