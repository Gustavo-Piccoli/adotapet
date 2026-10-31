import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  inject,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Data, NavigationEnd, Router, RouterOutlet, TitleStrategy } from '@angular/router';
import { filter, map, pairwise, startWith } from 'rxjs';
import { DEFAULT_TITLE } from '../core/title/app-title-strategy';
import { RouteAnnouncer } from '../core/title/route-announcer';
import { Footer } from './footer';
import { Header } from './header';

@Component({
  selector: 'app-shell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, Header, Footer],
  template: `
    <a
      href="#conteudo"
      (click)="skipToContent($event, main)"
      class="sr-only focus:not-sr-only focus:absolute focus:left-2 focus:top-2 focus:z-50 focus:rounded focus:bg-white focus:p-2"
    >
      Pular para o conteúdo
    </a>
    <div class="flex min-h-screen flex-col">
      <app-header />
      <main #main id="conteudo" tabindex="-1" class="flex-1 outline-none">
        <router-outlet />
      </main>
      <app-footer />
    </div>
    <div class="sr-only" role="status" aria-live="polite" data-testid="route-announcer">
      {{ announcer.message() }}
    </div>
  `,
})
export class Shell {
  private readonly router = inject(Router);
  private readonly titleStrategy = inject(TitleStrategy);
  private readonly main = viewChild.required<ElementRef<HTMLElement>>('main');

  protected readonly announcer = inject(RouteAnnouncer);

  constructor() {
    // On a real page change (path differs), move focus to <main> and announce the new page.
    // Query-param-only changes (filters, paging) keep focus where the user is.
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        map((e) => e.urlAfterRedirects.split(/[?#]/)[0]),
        startWith(null),
        pairwise(),
        filter(([previous, current]) => previous !== null && previous !== current),
        takeUntilDestroyed(inject(DestroyRef)),
      )
      .subscribe(() => {
        // Pages that load their title asynchronously (pet details) announce it themselves.
        if (!this.leafData()['announcesItself']) {
          // The router has not written document.title yet at NavigationEnd, so resolve it from the route.
          this.announcer.announce(
            this.titleStrategy.buildTitle(this.router.routerState.snapshot) ?? DEFAULT_TITLE,
          );
        }
        this.main().nativeElement.focus({ preventScroll: true });
      });
  }

  private leafData(): Data {
    let route = this.router.routerState.snapshot.root;
    while (route.firstChild) route = route.firstChild;
    return route.data;
  }

  protected skipToContent(event: Event, main: HTMLElement): void {
    event.preventDefault();
    main.focus();
  }
}
