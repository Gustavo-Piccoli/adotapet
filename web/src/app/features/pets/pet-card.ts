import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PetSummary } from '../../core/api/api.models';
import { AGE_GROUP_LABELS, SEX_LABELS, SIZE_LABELS, SPECIES_LABELS } from '../../shared/labels';
import { PawIcon } from '../../shared/paw-icon';

@Component({
  selector: 'app-pet-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, PawIcon],
  template: `
    <a
      [routerLink]="['/pets', pet().id]"
      class="card group block h-full transition duration-200 hover:-translate-y-1 hover:shadow-card-hover"
    >
      <div class="relative aspect-4/3 overflow-hidden bg-clay-50">
        @if (pet().photoUrl; as url) {
          <img
            [src]="url"
            [alt]="'Foto de ' + pet().name"
            loading="lazy"
            class="size-full object-cover transition duration-300 group-hover:scale-105"
          />
        } @else {
          <div class="grid size-full place-items-center text-clay-500" data-testid="photo-placeholder">
            <app-paw-icon class="size-20 opacity-60" />
            <span class="sr-only">Sem foto</span>
          </div>
        }
        <span class="chip chip-honey absolute left-3 top-3 shadow-sm">{{ species[pet().species] }}</span>
      </div>
      <div class="p-4">
        <h3 class="text-xl font-bold text-pine-900">{{ pet().name }}</h3>
        <div class="mt-2 flex flex-wrap gap-1.5">
          <span class="chip">{{ sex[pet().sex] }}</span>
          <span class="chip">{{ size[pet().size] }}</span>
          <span class="chip">{{ age[pet().ageGroup] }}</span>
        </div>
        <p class="mt-3 text-sm text-ink-soft">
          {{ pet().city }}/{{ pet().state }} · {{ pet().shelterName }}
        </p>
      </div>
    </a>
  `,
})
export class PetCard {
  readonly pet = input.required<PetSummary>();
  protected readonly species = SPECIES_LABELS;
  protected readonly sex = SEX_LABELS;
  protected readonly size = SIZE_LABELS;
  protected readonly age = AGE_GROUP_LABELS;
}
