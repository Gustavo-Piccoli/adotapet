import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthResult } from './auth.models';
import { AuthService, SESSION_STORAGE_KEY } from './auth.service';

function makeSession(overrides: Partial<AuthResult> = {}): AuthResult {
  return {
    accessToken: 'tok',
    expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    role: 'Adopter',
    profileId: 'p1',
    name: 'Ana',
    email: 'ana@x.com',
    ...overrides,
  };
}

function setup(): AuthService {
  TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
  return TestBed.inject(AuthService);
}

describe('AuthService', () => {
  beforeEach(() => localStorage.clear());

  it('starts logged out with empty storage', () => {
    const auth = setup();
    expect(auth.session()).toBeNull();
    expect(auth.isLoggedIn()).toBe(false);
  });

  it('login stores the session in signal and localStorage', () => {
    const auth = setup();
    const http = TestBed.inject(HttpTestingController);
    const session = makeSession({ role: 'Shelter' });

    auth.login({ email: 'a@b.com', password: 'x' }).subscribe();
    const req = http.expectOne('/api/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush(session);

    expect(auth.session()).toEqual(session);
    expect(auth.isShelter()).toBe(true);
    expect(auth.isAdopter()).toBe(false);
    expect(JSON.parse(localStorage.getItem(SESSION_STORAGE_KEY)!)).toEqual(session);
  });

  it('registerAdopter and registerShelter store the session', () => {
    const auth = setup();
    const http = TestBed.inject(HttpTestingController);

    auth
      .registerAdopter({ email: 'a@b.com', password: 'x', name: 'A', phone: '1', city: 'POA', state: 'RS' })
      .subscribe();
    http.expectOne('/api/auth/register/adopter').flush(makeSession());
    expect(auth.isAdopter()).toBe(true);

    auth
      .registerShelter({
        email: 's@b.com', password: 'x', name: 'S', cnpj: '1', city: 'POA', state: 'RS', phone: '1', description: 'd',
      })
      .subscribe();
    http.expectOne('/api/auth/register/shelter').flush(makeSession({ role: 'Shelter' }));
    expect(auth.isShelter()).toBe(true);
  });

  it('restores a valid persisted session', () => {
    const session = makeSession();
    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
    const auth = setup();
    expect(auth.session()).toEqual(session);
    expect(auth.isLoggedIn()).toBe(true);
  });

  it('drops an expired persisted session', () => {
    const expired = makeSession({ expiresAt: new Date(Date.now() - 1000).toISOString() });
    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(expired));
    const auth = setup();
    expect(auth.session()).toBeNull();
    expect(localStorage.getItem(SESSION_STORAGE_KEY)).toBeNull();
  });

  it('ignores corrupt persisted data', () => {
    localStorage.setItem(SESSION_STORAGE_KEY, '{not json');
    expect(setup().session()).toBeNull();
  });

  it('logout clears signal and storage', () => {
    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(makeSession()));
    const auth = setup();
    auth.logout();
    expect(auth.session()).toBeNull();
    expect(localStorage.getItem(SESSION_STORAGE_KEY)).toBeNull();
  });
});
