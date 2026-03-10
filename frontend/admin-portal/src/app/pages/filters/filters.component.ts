import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { BlockedCompany, BlockedKeyword, FetchOptions, FetchAndSyncResult } from '../../models/job.model';

type ActiveTab = 'companies' | 'keywords' | 'fetch';

@Component({
  selector: 'app-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './filters.component.html'
})
export class FiltersComponent implements OnInit {
  blockedCompanies = signal<BlockedCompany[]>([]);
  blockedKeywords = signal<BlockedKeyword[]>([]);
  companySuggestions = signal<string[]>([]);
  
  newCompany = '';
  newCompanyReason = '';
  newKeyword = '';
  newKeywordReason = '';
  companySearch = '';
  
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  activeTab = signal<ActiveTab>('companies');
  
  // Fetch state
  fetching = signal<boolean>(false);
  fetchResult = signal<FetchAndSyncResult | null>(null);
  fetchOptions: FetchOptions = {
    fetchGreenhouse: true,
    fetchLever: true,
    fetchWorkable: true,
    fetchJooble: true,
    fetchRemoteOk: true,
    fetchRemotive: true,
    fetchHimalayas: true,
    fetchJobicy: true,
    joobleMaxPages: 3,
  };

  fetchSources = [
    { key: 'fetchGreenhouse', label: 'Greenhouse' },
    { key: 'fetchLever', label: 'Lever' },
    { key: 'fetchWorkable', label: 'Workable' },
    { key: 'fetchJooble', label: 'Jooble' },
    { key: 'fetchRemoteOk', label: 'RemoteOK' },
    { key: 'fetchRemotive', label: 'Remotive' },
    { key: 'fetchHimalayas', label: 'Himalayas' },
    { key: 'fetchJobicy', label: 'Jobicy' },
  ];

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    Promise.all([
      this.apiService.getBlockedCompanies().toPromise(),
      this.apiService.getBlockedKeywords().toPromise()
    ]).then(([companies, keywords]) => {
      this.blockedCompanies.set(companies || []);
      this.blockedKeywords.set(keywords || []);
      this.loading.set(false);
    }).catch(err => {
      this.error.set(err.message || 'Failed to load data');
      this.loading.set(false);
    });
  }

  onCompanySearchChange(): void {
    if (this.companySearch.length >= 2) {
      this.apiService.getCompanySuggestions(this.companySearch).subscribe({
        next: (suggestions) => this.companySuggestions.set(suggestions),
        error: () => console.error('Failed to load suggestions')
      });
    } else {
      this.companySuggestions.set([]);
    }
    this.newCompany = this.companySearch;
  }

  selectSuggestion(company: string): void {
    this.newCompany = company;
    this.companySearch = '';
    this.companySuggestions.set([]);
  }

  handleAddCompany(): void {
    if (!this.newCompany.trim()) return;

    this.apiService.addBlockedCompany({
      companyName: this.newCompany.trim(),
      reason: this.newCompanyReason.trim() || undefined
    }).subscribe({
      next: (company) => {
        this.blockedCompanies.update(list => [company, ...list]);
        this.newCompany = '';
        this.newCompanyReason = '';
        this.companySearch = '';
        this.showSuccess('Company blocked successfully');
      },
      error: (err) => this.showError(err.error?.message || 'Failed to block company')
    });
  }

  handleToggleCompany(id: number): void {
    this.apiService.toggleBlockedCompany(id).subscribe({
      next: (updated) => {
        this.blockedCompanies.update(list => list.map(c => c.id === id ? updated : c));
      },
      error: (err) => this.showError(err.message || 'Failed to toggle company')
    });
  }

  handleDeleteCompany(id: number): void {
    if (!confirm('Are you sure you want to delete this blocked company?')) return;

    this.apiService.deleteBlockedCompany(id).subscribe({
      next: () => {
        this.blockedCompanies.update(list => list.filter(c => c.id !== id));
        this.showSuccess('Company unblocked successfully');
      },
      error: (err) => this.showError(err.message || 'Failed to delete company')
    });
  }

  handleAddKeyword(): void {
    if (!this.newKeyword.trim()) return;

    this.apiService.addBlockedKeyword({
      keyword: this.newKeyword.trim(),
      reason: this.newKeywordReason.trim() || undefined
    }).subscribe({
      next: (keyword) => {
        this.blockedKeywords.update(list => [keyword, ...list]);
        this.newKeyword = '';
        this.newKeywordReason = '';
        this.showSuccess('Keyword blocked successfully');
      },
      error: (err) => this.showError(err.error?.message || 'Failed to block keyword')
    });
  }

  handleToggleKeyword(id: number): void {
    this.apiService.toggleBlockedKeyword(id).subscribe({
      next: (updated) => {
        this.blockedKeywords.update(list => list.map(k => k.id === id ? updated : k));
      },
      error: (err) => this.showError(err.message || 'Failed to toggle keyword')
    });
  }

  handleDeleteKeyword(id: number): void {
    if (!confirm('Are you sure you want to delete this blocked keyword?')) return;

    this.apiService.deleteBlockedKeyword(id).subscribe({
      next: () => {
        this.blockedKeywords.update(list => list.filter(k => k.id !== id));
        this.showSuccess('Keyword unblocked successfully');
      },
      error: (err) => this.showError(err.message || 'Failed to delete keyword')
    });
  }

  handleFetchAndSync(): void {
    this.fetching.set(true);
    this.fetchResult.set(null);
    this.error.set(null);

    this.apiService.fetchAndSync('http://localhost:5200', this.fetchOptions).subscribe({
      next: (result) => {
        this.fetchResult.set(result);
        this.showSuccess(`Fetched ${result.fetchResult.totalFetched} jobs, imported ${result.syncResult.imported} new jobs`);
        this.fetching.set(false);
      },
      error: (err) => {
        this.showError(err.error?.message || err.message || 'Fetch failed');
        this.fetching.set(false);
      }
    });
  }

  setActiveTab(tab: ActiveTab): void {
    this.activeTab.set(tab);
  }

  getActiveCompaniesCount(): number {
    return this.blockedCompanies().filter(c => c.isActive).length;
  }

  getActiveKeywordsCount(): number {
    return this.blockedKeywords().filter(k => k.isActive).length;
  }

  getFetchSourceStats(): [string, number][] {
    const result = this.fetchResult();
    if (!result?.fetchResult?.sourceStats) return [];
    return Object.entries(result.fetchResult.sourceStats);
  }

  toggleFetchOption(key: string): void {
    (this.fetchOptions as any)[key] = !(this.fetchOptions as any)[key];
  }

  private showSuccess(message: string): void {
    this.success.set(message);
    setTimeout(() => this.success.set(null), 3000);
  }

  private showError(message: string): void {
    this.error.set(message);
    setTimeout(() => this.error.set(null), 3000);
  }
}
