import { Injectable, signal, PLATFORM_ID, Inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'egypt-tech-jobs-theme';
  private isBrowser: boolean;
  
  isDarkMode = signal(false);

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
    if (this.isBrowser) {
      this.isDarkMode.set(this.getInitialTheme());
    }
  }

  private getInitialTheme(): boolean {
    if (!this.isBrowser) return false;
    const saved = localStorage.getItem(this.THEME_KEY);
    if (saved !== null) {
      return saved === 'dark';
    }
    // Default to light theme
    return false;
  }

  toggleTheme(): void {
    const newValue = !this.isDarkMode();
    this.isDarkMode.set(newValue);
    if (this.isBrowser) {
      localStorage.setItem(this.THEME_KEY, newValue ? 'dark' : 'light');
    }
    this.applyTheme();
  }

  applyTheme(): void {
    if (!this.isBrowser) return;
    if (this.isDarkMode()) {
      document.body.classList.add('dark-theme');
      document.body.classList.remove('light-theme');
    } else {
      document.body.classList.add('light-theme');
      document.body.classList.remove('dark-theme');
    }
  }

  initTheme(): void {
    if (this.isBrowser) {
      this.applyTheme();
    }
  }
}
