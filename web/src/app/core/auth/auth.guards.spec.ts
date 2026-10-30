import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CanActivateFn, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { AuthResult, Role } from './auth.models';
import { adopterGuard, authGuard, shelterGuard } from './auth.guards';
import { AuthService } from './auth.service';

function run(guard: CanActivateFn, url = '/minha-area'): boolean | UrlTree {
  return TestBed.runInInjectionContext(() =>
    guard({} as never, { url } as RouterStateSnapshot),
  ) as boolean | UrlTree;
}

function loginAs(role: Role): void {
  const result: AuthResult = {
    accessToken: 't',
    expiresAt: new Date(Date.now() + 3600_000).toISOString(),
    role,
    profileId: 'p',
    name: 'N',
    email: 'n@x.com',
  };
  TestBed.inject(AuthService).session.set(result);
}

describe('auth guards', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it('authGuard redirects anonymous users to /entrar with returnUrl', () => {
    const result = run(authGuard, '/meus-pedidos');
    expect(result instanceof UrlTree).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe(
      '/entrar?returnUrl=%2Fmeus-pedidos',
    );
  });

  it('authGuard allows any logged user', () => {
    loginAs('Adopter');
    expect(run(authGuard)).toBe(true);
  });

  it('shelterGuard redirects an adopter to /', () => {
    loginAs('Adopter');
    const result = run(shelterGuard);
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe('/');
  });

  it('shelterGuard allows a shelter and redirects anonymous to /entrar', () => {
    expect(TestBed.inject(Router).serializeUrl(run(shelterGuard, '/ong') as UrlTree)).toBe(
      '/entrar?returnUrl=%2Fong',
    );
    loginAs('Shelter');
    expect(run(shelterGuard)).toBe(true);
  });

  it('adopterGuard redirects a shelter to / and allows an adopter', () => {
    loginAs('Shelter');
    expect(TestBed.inject(Router).serializeUrl(run(adopterGuard) as UrlTree)).toBe('/');
    loginAs('Adopter');
    expect(run(adopterGuard)).toBe(true);
  });
});
