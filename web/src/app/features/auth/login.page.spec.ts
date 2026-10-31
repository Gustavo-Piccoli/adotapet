import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AuthResult } from '../../core/auth/auth.models';
import { AuthService } from '../../core/auth/auth.service';
import { guestGuard } from '../../core/auth/auth.guards';
import { ToastService } from '../../core/toast/toast.service';
import { LoginPage } from './login.page';

@Component({ template: 'stub' })
class Stub {}

const RESULT = (role: 'Shelter' | 'Adopter'): AuthResult => ({
  accessToken: 't',
  expiresAt: '2999-01-01T00:00:00Z',
  role,
  profileId: 'x',
  name: 'N',
  email: 'n@x.com',
});

describe('LoginPage', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;

  beforeEach(async () => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'entrar', canActivate: [guestGuard], component: LoginPage },
          { path: 'pets/:id', component: Stub },
          { path: 'ong/pets', component: Stub },
          { path: '', pathMatch: 'full', component: Stub },
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
  });

  afterEach(() => http.verify());

  const submit = (el: HTMLElement, email = 'a@b.com', password = 'senha1234') => {
    const set = (id: string, value: string) => {
      const input = el.querySelector<HTMLInputElement>(id)!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
    };
    set('#l-email', email);
    set('#l-password', password);
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
  };

  it('redirects to the returnUrl after login', async () => {
    await harness.navigateByUrl('/entrar?returnUrl=/pets/p1');
    submit(harness.routeNativeElement as HTMLElement);
    http.expectOne('/api/auth/login').flush(RESULT('Adopter'));
    await harness.fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/pets/p1');
  });

  it('ignores an external returnUrl and goes to the role home', async () => {
    await harness.navigateByUrl('/entrar?returnUrl=//evil.com');
    submit(harness.routeNativeElement as HTMLElement);
    http.expectOne('/api/auth/login').flush(RESULT('Shelter'));
    await harness.fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/ong/pets');
  });

  it('shows the API detail in a toast on 401 and stays on the page', async () => {
    await harness.navigateByUrl('/entrar');
    submit(harness.routeNativeElement as HTMLElement);
    http
      .expectOne('/api/auth/login')
      .flush({ detail: 'E-mail ou senha inválidos.' }, { status: 401, statusText: 'Unauthorized' });
    await harness.fixture.whenStable();
    expect(TestBed.inject(ToastService).toasts()[0].message).toBe('E-mail ou senha inválidos.');
    expect(TestBed.inject(Router).url).toBe('/entrar');
  });

  it('does not call the API when the form is invalid', async () => {
    await harness.navigateByUrl('/entrar');
    submit(harness.routeNativeElement as HTMLElement, 'not-an-email', '');
    http.expectNone('/api/auth/login');
  });

  it('redirects a logged-in user away from /entrar', async () => {
    TestBed.inject(AuthService).session.set(RESULT('Shelter'));
    await harness.navigateByUrl('/entrar');
    expect(TestBed.inject(Router).url).toBe('/ong/pets');
  });
});
