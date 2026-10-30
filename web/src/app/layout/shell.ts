import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
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
      <main #main id="conteudo" tabindex="-1" class="flex-1">
        <router-outlet />
      </main>
      <app-footer />
    </div>
  `,
})
export class Shell {
  protected skipToContent(event: Event, main: HTMLElement): void {
    event.preventDefault();
    main.focus();
  }
}
