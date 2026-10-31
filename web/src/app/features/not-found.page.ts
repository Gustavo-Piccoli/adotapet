import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EmptyState } from '../shared/empty-state';

@Component({
  selector: 'app-not-found-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, EmptyState],
  template: `
    <section class="container-page py-12">
      <h1 class="sr-only">Página não encontrada</h1>
      <app-empty-state message="Página não encontrada.">
        <p class="mt-2">O endereço que você acessou não existe ou foi removido.</p>
        <a routerLink="/" class="btn btn-primary mt-5">Ver pets para adotar</a>
      </app-empty-state>
    </section>
  `,
})
export class NotFoundPage {}
