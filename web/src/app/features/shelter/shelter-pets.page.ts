import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ShelterPet } from '../../core/api/api.models';
import { ShelterApi } from '../../core/api/shelter.api';
import { ErrorBanner } from '../../shared/error-banner';
import { EmptyState } from '../../shared/empty-state';
import { AGE_GROUP_LABELS, PET_STATUS_LABELS, SPECIES_LABELS } from '../../shared/labels';

@Component({
  selector: 'app-shelter-pets-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, EmptyState, ErrorBanner],
  template: `
    <section class="container-page py-8">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <h1 class="text-3xl font-bold text-pine-900">Meus pets</h1>
        <a routerLink="/ong/pets/novo" class="btn btn-primary">Cadastrar pet</a>
      </div>

      @if (loading()) {
        <p class="sr-only" role="status">Carregando…</p>
        <div class="card mt-6 h-32 animate-pulse bg-cream-200" aria-hidden="true"></div>
      } @else if (failed()) {
        <app-error-banner
          class="mt-6 block"
          message="Não foi possível carregar seus pets. Tente novamente em instantes."
          (retry)="load()"
        />
      } @else if (pets().length === 0) {
        <div class="mt-6">
          <app-empty-state message="Você ainda não cadastrou nenhum pet.">
            <a routerLink="/ong/pets/novo" class="btn btn-primary mt-5">Cadastrar pet</a>
          </app-empty-state>
        </div>
      } @else {
        <ul class="mt-6 grid gap-3">
          @for (p of pets(); track p.id) {
            <li class="card flex flex-wrap items-center gap-4 p-4" data-testid="shelter-pet">
              <div class="size-16 shrink-0 overflow-hidden rounded-card bg-clay-50">
                @if (p.photoUrl; as url) {
                  <img [src]="url" [alt]="'Foto de ' + p.name" class="size-full object-cover" />
                }
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-lg font-bold text-pine-900">{{ p.name }}</p>
                <p class="text-sm text-ink-soft">{{ species[p.species] }} · {{ age[p.ageGroup] }}</p>
              </div>
              <span class="chip" [class.chip-clay]="p.status === 'Adopted'">{{
                status[p.status]
              }}</span>
              <a [routerLink]="['/ong/pets', p.id, 'editar']" class="btn btn-secondary">Editar</a>
            </li>
          }
        </ul>
      }
    </section>
  `,
})
export class ShelterPetsPage {
  protected readonly species = SPECIES_LABELS;
  protected readonly age = AGE_GROUP_LABELS;
  protected readonly status = PET_STATUS_LABELS;
  protected readonly pets = signal<ShelterPet[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);

  private readonly api = inject(ShelterApi);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.failed.set(false);
    this.api
      .listPets()
      .subscribe({
        next: (list) => {
          this.pets.set(list);
          this.loading.set(false);
        },
        error: () => {
          this.failed.set(true);
          this.loading.set(false);
        },
      });
  }
}
