import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdoptionRequestsApi } from '../../core/api/adoption-requests.api';
import { AdopterRequest } from '../../core/api/api.models';
import { EmptyState } from '../../shared/empty-state';
import { formatDate } from '../../shared/format';
import { REQUEST_STATUS_LABELS } from '../../shared/labels';
import { PawIcon } from '../../shared/paw-icon';

@Component({
  selector: 'app-my-requests-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, EmptyState, PawIcon],
  template: `
    <section class="container-page py-8">
      <h1 class="text-3xl font-bold text-pine-900">Meus pedidos</h1>

      @if (loading()) {
        <div class="card mt-6 h-32 animate-pulse bg-cream-200" aria-hidden="true"></div>
      } @else if (failed()) {
        <div class="mt-6 rounded-card bg-red-50 px-5 py-4 text-red-800" role="alert">
          Não foi possível carregar seus pedidos. Tente novamente em instantes.
        </div>
      } @else if (requests().length === 0) {
        <div class="mt-6">
          <app-empty-state message="Você ainda não fez nenhum pedido de adoção.">
            <a routerLink="/" class="btn btn-primary mt-5">Ver pets para adotar</a>
          </app-empty-state>
        </div>
      } @else {
        <ul class="mt-6 grid gap-4">
          @for (r of requests(); track r.id) {
            <li class="card flex gap-4 p-4" data-testid="request-item">
              <div class="size-24 shrink-0 overflow-hidden rounded-card bg-clay-50">
                @if (r.petPhotoUrl; as url) {
                  <img [src]="url" [alt]="'Foto de ' + r.petName" class="size-full object-cover" />
                } @else {
                  <div class="grid size-full place-items-center text-clay-500">
                    <app-paw-icon class="size-10 opacity-60" />
                    <span class="sr-only">Sem foto</span>
                  </div>
                }
              </div>
              <div class="min-w-0 flex-1">
                <div class="flex flex-wrap items-center gap-2">
                  <a
                    [routerLink]="['/pets', r.petId]"
                    class="text-xl font-bold text-pine-900 underline"
                    >{{ r.petName }}</a
                  >
                  <span
                    class="chip"
                    [class.chip-honey]="r.status === 'Pending'"
                    [class.chip-clay]="r.status === 'Rejected'"
                    >{{ statusLabels[r.status] }}</span
                  >
                </div>
                <p class="text-sm text-ink-soft">{{ r.shelterName }}</p>
                <p class="text-sm text-ink-soft">Enviado em {{ date(r.createdAt) }}</p>
              </div>
            </li>
          }
        </ul>
      }
    </section>
  `,
})
export class MyRequestsPage {
  protected readonly statusLabels = REQUEST_STATUS_LABELS;
  protected readonly date = formatDate;
  protected readonly requests = signal<AdopterRequest[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);

  constructor() {
    inject(AdoptionRequestsApi)
      .listMine()
      .subscribe({
        next: (list) => {
          this.requests.set(list);
          this.loading.set(false);
        },
        error: () => {
          this.failed.set(true);
          this.loading.set(false);
        },
      });
  }
}
