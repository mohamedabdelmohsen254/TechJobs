import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';
import { User, LoginResponse } from '../models/job.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = `${environment.apiUrl}/api`;
  
  private userSignal = signal<User | null>(null);
  private tokenSignal = signal<string | null>(this.getStoredToken());
  private loadingSignal = signal<boolean>(true);

  readonly user = this.userSignal.asReadonly();
  readonly token = this.tokenSignal.asReadonly();
  readonly isLoading = this.loadingSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenSignal() && !!this.userSignal());

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initAuth();
  }

  private getStoredToken(): string | null {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem('token');
    }
    return null;
  }

  private initAuth(): void {
    const savedToken = this.getStoredToken();
    if (savedToken) {
      this.getCurrentUser().subscribe({
        next: (user) => {
          this.userSignal.set(user);
          this.loadingSignal.set(false);
        },
        error: () => {
          this.clearAuth();
          this.loadingSignal.set(false);
        }
      });
    } else {
      this.loadingSignal.set(false);
    }
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API_URL}/auth/login`, { email, password }).pipe(
      tap((response) => {
        if (response.success && response.token) {
          localStorage.setItem('token', response.token);
          this.tokenSignal.set(response.token);
          this.userSignal.set(response.user || null);
        }
      })
    );
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.API_URL}/auth/me`);
  }

  logout(): void {
    this.clearAuth();
    this.router.navigate(['/login']);
  }

  private clearAuth(): void {
    localStorage.removeItem('token');
    this.tokenSignal.set(null);
    this.userSignal.set(null);
  }
}
