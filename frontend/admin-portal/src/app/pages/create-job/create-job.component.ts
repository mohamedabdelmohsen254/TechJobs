import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { CreateJobDto } from '../../models/job.model';

@Component({
  selector: 'app-create-job',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-job.component.html'
})
export class CreateJobComponent {
  error = signal<string | null>(null);
  isSubmitting = signal<boolean>(false);

  job: CreateJobDto = {
    title: '',
    company: '',
    location: '',
    country: '',
    city: '',
    workType: '',
    applyUrl: '',
    source: 'Manual',
    description: '',
    salaryRange: '',
    tags: ''
  };

  constructor(
    private apiService: ApiService,
    private router: Router
  ) {}

  onSubmit(): void {
    this.error.set(null);
    this.isSubmitting.set(true);

    const jobData: CreateJobDto = {
      title: this.job.title,
      company: this.job.company,
      applyUrl: this.job.applyUrl,
      source: 'Manual',
      location: this.job.location || undefined,
      country: this.job.country || undefined,
      city: this.job.city || undefined,
      workType: this.job.workType || undefined,
      description: this.job.description || undefined,
      salaryRange: this.job.salaryRange || undefined,
      tags: this.job.tags || undefined
    };

    this.apiService.createJob(jobData).subscribe({
      next: () => {
        this.router.navigate(['/jobs']);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to create job');
        this.isSubmitting.set(false);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/jobs']);
  }
}
