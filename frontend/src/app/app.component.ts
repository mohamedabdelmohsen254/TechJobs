import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ThemeService } from './services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  template: `
    <nav class="navbar">
      <div class="container">
        <a routerLink="/" class="brand">
          <span class="brand-icon">💼</span>
          Egypt Tech Jobs
        </a>
        <div class="nav-links">
          <a routerLink="/jobs" routerLinkActive="active" class="nav-link">
            <span class="icon">📋</span> Jobs
          </a>
          <a routerLink="/fetch" routerLinkActive="active" class="nav-link">
            <span class="icon">🔄</span> Fetch Jobs
          </a>
          <a routerLink="/stats" routerLinkActive="active" class="nav-link">
            <span class="icon">📊</span> Stats
          </a>
          <button (click)="toggleTheme()" class="theme-toggle" [title]="themeService.isDarkMode() ? 'Switch to Light Mode' : 'Switch to Dark Mode'">
            <span class="icon">{{ themeService.isDarkMode() ? '☀️' : '🌙' }}</span>
          </button>
        </div>
      </div>
    </nav>
    <main class="main-content">
      <router-outlet></router-outlet>
    </main>
    <footer class="footer">
      <div class="container">
        <p>&copy; 2026 Egypt Tech Jobs. Powered by Angular & .NET</p>
      </div>
    </footer>
  `,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
    }

    .navbar {
      background: var(--navbar-bg);
      padding: 1rem 0;
      box-shadow: 0 2px 10px var(--shadow-color);
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 0 2rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .brand {
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--navbar-text);
      text-decoration: none;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .brand-icon {
      font-size: 1.8rem;
    }

    .nav-links {
      display: flex;
      gap: 1rem;
      align-items: center;
    }

    .nav-link {
      color: var(--navbar-link);
      text-decoration: none;
      padding: 0.5rem 1rem;
      border-radius: 8px;
      transition: all 0.3s ease;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .nav-link:hover {
      color: var(--navbar-text);
      background: rgba(255, 255, 255, 0.15);
    }

    .nav-link.active {
      color: var(--navbar-text);
      background: var(--navbar-active);
    }

    .theme-toggle {
      background: rgba(255, 255, 255, 0.15);
      border: none;
      padding: 0.5rem 0.75rem;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.3s ease;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .theme-toggle:hover {
      background: rgba(255, 255, 255, 0.25);
      transform: scale(1.1);
    }

    .theme-toggle .icon {
      font-size: 1.2rem;
    }

    .icon {
      font-size: 1.1rem;
    }

    .main-content {
      flex: 1;
      background: var(--bg-primary);
      padding: 2rem 0;
    }

    .footer {
      background: var(--bg-secondary);
      color: var(--text-secondary);
      padding: 1.5rem 0;
      text-align: center;
      border-top: 1px solid var(--border-color);
    }

    .footer p {
      margin: 0;
    }

    @media (max-width: 768px) {
      .container {
        flex-direction: column;
        gap: 1rem;
      }

      .nav-links {
        flex-wrap: wrap;
        justify-content: center;
      }
    }
  `]
})
export class AppComponent implements OnInit {
  constructor(public themeService: ThemeService) {}

  ngOnInit() {
    this.themeService.initTheme();
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}
