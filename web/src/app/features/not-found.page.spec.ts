import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from '../app.routes';

describe('NotFoundPage', () => {
  it('is rendered by the wildcard route with a link back to the showcase', async () => {
    TestBed.configureTestingModule({ providers: [provideRouter(routes)] });
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/nao-existe');
    harness.detectChanges();
    const el = harness.routeNativeElement as HTMLElement;
    expect(el.textContent).toContain('Página não encontrada');
    expect(el.querySelector('a[href="/"]')).not.toBeNull();
    expect(TestBed.inject(Router).url).toBe('/nao-existe');
  });
});
