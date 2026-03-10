import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { Job, UpdateJobDto } from '../../models/job.model';

@Component({
  selector: 'app-edit-job',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-job.component.html'
})
export class EditJobComponent implements OnInit {
  error = signal<string | null>(null);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  job = signal<Job | null>(null);

  private jobId!: number;

  // Form model
  formData = {
    title: '',
    company: '',
    location: '',
    country: '',
    city: '',
    workType: '',
    applyUrl: '',
    description: '',
    salaryRange: '',
    tags: '',
    isActive: 'true'
  };

  constructor(
    private apiService: ApiService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.jobId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadJob();
  }

  loadJob(): void {
    this.apiService.getJob(this.jobId).subscribe({
      next: (job) => {
        this.job.set(job);
        this.formData = {
          title: job.title,
          company: job.company,
          location: job.location || '',
          country: job.country || '',
          city: job.city || '',
          workType: job.workType || '',
          applyUrl: job.applyUrl,
          description: job.description || '',
          salaryRange: job.salaryRange || '',
          tags: job.tags || '',
          isActive: job.isActive.toString()
        };
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Job not found');
        this.isLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    this.error.set(null);
    this.isSubmitting.set(true);

    const updates: UpdateJobDto = {
      title: this.formData.title,
      company: this.formData.company,
      applyUrl: this.formData.applyUrl,
      location: this.formData.location || undefined,
      country: this.formData.country || undefined,
      city: this.formData.city || undefined,
      workType: this.formData.workType || undefined,
      description: this.formData.description || undefined,
      salaryRange: this.formData.salaryRange || undefined,
      tags: this.formData.tags || undefined,
      isActive: this.formData.isActive === 'true'
    };

    this.apiService.updateJob(this.jobId, updates).subscribe({
      next: () => {
        this.router.navigate(['/jobs']);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to update job');
        this.isSubmitting.set(false);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/jobs']);
  }
}
