import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface NavItem {
  path: string;
  label: string;
  icon: string;
  exact: boolean;
}

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './layout.component.html'
})
export class LayoutComponent {
  navItems: NavItem[] = [
    { path: '/', label: 'Dashboard', icon: '📊', exact: true },
    { path: '/jobs', label: 'Jobs', icon: '💼', exact: true },
    { path: '/jobs/create', label: 'Add Job', icon: '➕', exact: true },
    { path: '/filters', label: 'Filters & Fetch', icon: '🔧', exact: true },
  ];

  constructor(public authService: AuthService) {}

  logout(): void {
    this.authService.logout();
  }

  getUserInitial(): string {
    const user = this.authService.user();
    return user?.fullName?.charAt(0) || user?.username?.charAt(0) || 'A';
  }
}
