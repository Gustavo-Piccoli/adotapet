import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthResult } from '../auth/auth.models';
import { AuthService } from '../auth/auth.service';
import { authInterceptor } from './auth.interceptor';

@Component({ template: '' })
class Blank {}

const session: AuthResult = {
  accessToken: 'abc123',
  expiresAt: new Date(Date.now() + 3600_000).toISOString(),
  role: 'Adopter',
  profileId: 'p',
  name: 'N',
  email: 'n@x.com',
};

describe('authInterceptor', () => {
  let http: HttpClient;
  let ctrl: HttpTestingController;
  let auth: AuthService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'pets/:id', component: Blank },
          { path: 'entrar', component: Blank },
        ]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpClient);
    ctrl = TestBed.inject(HttpTestingController);
    auth = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  it('adds the Bearer token when logged in', () => {
    auth.session.set(session);
    http.get('/api/me').subscribe();
    const req = ctrl.expectOne('/api/me');
    expect(req.request.headers.get('Authorization')).toBe('Bearer abc123');
    req.flush({});
  });

  it('does not add a header when logged out', () => {
    http.get('/api/pets').subscribe();
    const req = ctrl.expectOne('/api/pets');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('on 401 logs out and navigates to /entrar with the current url', async () => {
    auth.session.set(session);
    await router.navigateByUrl('/pets/42');
    const logout = vi.spyOn(auth, 'logout');
    const navigate = vi.spyOn(router, 'navigate');
    let status = 0;

    http.get('/api/me').subscribe({ error: (e) => (status = e.status) });
    ctrl.expectOne('/api/me').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(status).toBe(401);
    expect(logout).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/entrar'], { queryParams: { returnUrl: '/pets/42' } });
    expect(auth.session()).toBeNull();
  });

  it('does not log out on 401 from an auth endpoint (wrong password)', () => {
    auth.session.set(session);
    const logout = vi.spyOn(auth, 'logout');
    http.post('/api/auth/login', {}).subscribe({ error: () => undefined });
    ctrl.expectOne('/api/auth/login').flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(logout).not.toHaveBeenCalled();
  });
});
