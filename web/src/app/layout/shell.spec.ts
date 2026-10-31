import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { Shell } from './shell';

@Component({ template: '<h1>x</h1>' })
class Page {}

describe('Shell route focus', () => {
  let harness: RouterTestingHarness;
  let host: HTMLElement;
  let shell: ComponentFixture<Shell>;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'a', title: 'Página A — AdotaPet', component: Page },
          { path: 'b', title: 'Página B — AdotaPet', component: Page },
        ]),
      ],
    });
    const fixture = TestBed.createComponent(Shell);
    host = fixture.nativeElement;
    fixture.detectChanges();
    shell = fixture;
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
    shell.detectChanges();
    expect(host.querySelector('[data-testid="route-announcer"]')?.textContent).toContain(
      'Página B — AdotaPet',
    );

    (document.activeElement as HTMLElement).blur();
    await harness.navigateByUrl('/b?page=2');
    expect(document.activeElement).not.toBe(main);
  });
});
