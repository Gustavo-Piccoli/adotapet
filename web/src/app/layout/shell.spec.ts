import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { RouteAnnouncer } from '../core/title/route-announcer';
import { Shell } from './shell';

@Component({ template: '<h1>x</h1>' })
class Page {}

describe('Shell route focus', () => {
  let harness: RouterTestingHarness;
  let host: HTMLElement;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'a', title: 'Página A — AdotPet', component: Page },
          {
            path: 'p/:id',
            title: 'Pet — AdotPet',
            data: { announcesItself: true },
            component: Page,
          },
          { path: 'b', title: 'Página B — AdotPet', component: Page },
        ]),
      ],
    });
    const fixture = TestBed.createComponent(Shell);
    host = fixture.nativeElement;
    fixture.detectChanges();

    harness = await RouterTestingHarness.create();
    // Created before the harness, whose root handling can detach it from the document.
    document.body.appendChild(host);
  });

  it('does not steal focus on the first navigation', async () => {
    await harness.navigateByUrl('/a');
    expect(document.activeElement).not.toBe(host.querySelector('main'));
  });

  it('focuses main and announces the title when the path changes, not on query-only changes', async () => {
    await harness.navigateByUrl('/a');
    await harness.navigateByUrl('/b');
    const main = host.querySelector('main')!;
    expect(document.activeElement).toBe(main);

    (document.activeElement as HTMLElement).blur();
    await harness.navigateByUrl('/b?page=2');
    expect(document.activeElement).not.toBe(main);
  });

  it('re-announces identical titles and leaves self-announcing routes to the page', async () => {
    vi.useFakeTimers();
    try {
      const announcer = TestBed.inject(RouteAnnouncer);
      await harness.navigateByUrl('/a');
      await harness.navigateByUrl('/b');
      vi.advanceTimersByTime(60);
      expect(announcer.message()).toBe('Página B — AdotPet');

      announcer.announce('Página B — AdotPet');
      expect(announcer.message()).toBe(''); // cleared first, so the repeat is announced again
      vi.advanceTimersByTime(60);
      expect(announcer.message()).toBe('Página B — AdotPet');

      await harness.navigateByUrl('/p/1');
      vi.advanceTimersByTime(60);
      expect(announcer.message()).toBe('Página B — AdotPet'); // untouched: the page announces itself
    } finally {
      vi.useRealTimers();
    }
  });
});
