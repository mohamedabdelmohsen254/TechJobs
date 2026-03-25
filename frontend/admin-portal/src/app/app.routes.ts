import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./components/layout/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'jobs',
        loadComponent: () => import('./pages/job-list/job-list.component').then(m => m.JobListComponent)
      },
      {
        path: 'jobs/create',
        loadComponent: () => import('./pages/create-job/create-job.component').then(m => m.CreateJobComponent)
      },
      {
        path: 'jobs/:id/edit',
        loadComponent: () => import('./pages/edit-job/edit-job.component').then(m => m.EditJobComponent)
      },
      {
        path: 'filters',
        loadComponent: () => import('./pages/filters/filters.component').then(m => m.FiltersComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
