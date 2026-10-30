import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthResult, LoginDto, RegisterAdopterDto, RegisterShelterDto } from './auth.models';

export const SESSION_STORAGE_KEY = 'adotapet.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  readonly session = signal<AuthResult | null>(this.load());
  readonly isLoggedIn = computed(() => this.session() !== null);
  readonly role = computed(() => this.session()?.role ?? null);
  readonly isShelter = computed(() => this.role() === 'Shelter');
  readonly isAdopter = computed(() => this.role() === 'Adopter');

  login(dto: LoginDto): Observable<AuthResult> {
    return this.http.post<AuthResult>('/api/auth/login', dto).pipe(tap((r) => this.store(r)));
  }

  registerShelter(dto: RegisterShelterDto): Observable<AuthResult> {
    return this.http
      .post<AuthResult>('/api/auth/register/shelter', dto)
      .pipe(tap((r) => this.store(r)));
  }

  registerAdopter(dto: RegisterAdopterDto): Observable<AuthResult> {
    return this.http
      .post<AuthResult>('/api/auth/register/adopter', dto)
      .pipe(tap((r) => this.store(r)));
  }

  logout(): void {
    this.session.set(null);
    try {
      localStorage.removeItem(SESSION_STORAGE_KEY);
    } catch {
      // storage unavailable: in-memory state is already cleared
    }
  }

  private store(result: AuthResult): void {
    this.session.set(result);
    try {
      localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(result));
    } catch {
      // storage unavailable: session lives in memory only
    }
  }

  private load(): AuthResult | null {
    try {
      const raw = localStorage.getItem(SESSION_STORAGE_KEY);
      if (!raw) return null;
      const session = JSON.parse(raw) as AuthResult;
      if (!session.accessToken || !(Date.parse(session.expiresAt) > Date.now())) {
        localStorage.removeItem(SESSION_STORAGE_KEY);
        return null;
      }
      return session;
    } catch {
      return null;
    }
  }
}
