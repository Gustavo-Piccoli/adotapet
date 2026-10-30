import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { SearchPetsParams } from '../../core/api/api.models';
import {
  AGE_GROUP_LABELS,
  SEX_LABELS,
  SIZE_LABELS,
  SPECIES_LABELS,
  UFS,
} from '../../shared/labels';

export type PetFilterValues = Pick<
  SearchPetsParams,
  'species' | 'sex' | 'size' | 'ageGroup' | 'state' | 'city'
>;

@Component({
  selector: 'app-pet-filters',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form
      class="card grid gap-4 p-4 sm:grid-cols-2 sm:p-5 lg:grid-cols-6"
      aria-label="Filtrar pets"
      (submit)="$event.preventDefault()"
    >
      <div>
        <label class="field-label" for="f-species">Espécie</label>
        <select id="f-species" class="field-input" (change)="set('species', $event)">
          <option value="" [selected]="!value().species">Todas</option>
          @for (o of species; track o[0]) {
            <option [value]="o[0]" [selected]="value().species === o[0]">{{ o[1] }}</option>
          }
        </select>
      </div>
      <div>
        <label class="field-label" for="f-size">Porte</label>
        <select id="f-size" class="field-input" (change)="set('size', $event)">
          <option value="" [selected]="!value().size">Todos</option>
          @for (o of sizes; track o[0]) {
            <option [value]="o[0]" [selected]="value().size === o[0]">{{ o[1] }}</option>
          }
        </select>
      </div>
      <div>
        <label class="field-label" for="f-sex">Sexo</label>
        <select id="f-sex" class="field-input" (change)="set('sex', $event)">
          <option value="" [selected]="!value().sex">Todos</option>
          @for (o of sexes; track o[0]) {
            <option [value]="o[0]" [selected]="value().sex === o[0]">{{ o[1] }}</option>
          }
        </select>
      </div>
      <div>
        <label class="field-label" for="f-age">Idade</label>
        <select id="f-age" class="field-input" (change)="set('ageGroup', $event)">
          <option value="" [selected]="!value().ageGroup">Todas</option>
          @for (o of ages; track o[0]) {
            <option [value]="o[0]" [selected]="value().ageGroup === o[0]">{{ o[1] }}</option>
          }
        </select>
      </div>
      <div>
        <label class="field-label" for="f-state">UF</label>
        <select id="f-state" class="field-input" (change)="set('state', $event)">
          <option value="" [selected]="!value().state">Todas</option>
          @for (uf of ufs; track uf) {
            <option [value]="uf" [selected]="value().state === uf">{{ uf }}</option>
          }
        </select>
      </div>
      <div>
        <label class="field-label" for="f-city">Cidade</label>
        <input
          id="f-city"
          type="text"
          class="field-input"
          placeholder="Ex.: Porto Alegre"
          [value]="value().city ?? ''"
          (change)="set('city', $event)"
        />
      </div>
    </form>
  `,
})
export class PetFilters {
  readonly value = input.required<PetFilterValues>();
  readonly valueChange = output<PetFilterValues>();

  protected readonly species = Object.entries(SPECIES_LABELS);
  protected readonly sizes = Object.entries(SIZE_LABELS);
  protected readonly sexes = Object.entries(SEX_LABELS);
  protected readonly ages = Object.entries(AGE_GROUP_LABELS);
  protected readonly ufs = UFS;

  protected set(key: keyof PetFilterValues, event: Event): void {
    const raw = (event.target as HTMLInputElement | HTMLSelectElement).value.trim();
    this.valueChange.emit({ ...this.value(), [key]: raw === '' ? undefined : raw });
  }
}
