import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AgeGroup, PetInput, Sex, ShelterPet, Size, Species } from '../../core/api/api.models';
import { ShelterApi } from '../../core/api/shelter.api';
import { ToastService } from '../../core/toast/toast.service';
import { EmptyState } from '../../shared/empty-state';
import { FieldError } from '../../shared/field-error';
import { AGE_GROUP_LABELS, SEX_LABELS, SIZE_LABELS, SPECIES_LABELS } from '../../shared/labels';

export const MAX_PHOTO_BYTES = 5 * 1024 * 1024;
const PHOTO_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

export function photoError(file: File): string | null {
  if (!PHOTO_TYPES.includes(file.type)) return 'A foto deve ser JPEG, PNG ou WebP.';
  if (file.size > MAX_PHOTO_BYTES) return 'A foto deve ter no máximo 5 MB.';
  return null;
}

/** Length validator on the trimmed value, so whitespace-only input is "required". */
function trimmedLength(min: number, max: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    const length = String(control.value ?? '').trim().length;
    if (length === 0) return { required: true };
    if (length < min) return { minlength: { requiredLength: min, actualLength: length } };
    if (length > max) return { maxlength: { requiredLength: max, actualLength: length } };
    return null;
  };
}

function options<T extends string>(labels: Record<T, string>): { value: T; label: string }[] {
  return (Object.keys(labels) as T[]).map((value) => ({ value, label: labels[value] }));
}

@Component({
  selector: 'app-pet-form-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, FieldError, EmptyState],
  template: `
    <section class="container-page py-8">
      <a routerLink="/ong/pets" class="btn btn-ghost mb-4 -ml-3">&larr; Voltar para meus pets</a>

      @if (loading()) {
        <div class="card h-96 animate-pulse bg-cream-200" aria-hidden="true"></div>
      } @else if (notFound()) {
        <app-empty-state message="Pet não encontrado.">
          <a routerLink="/ong/pets" class="btn btn-primary mt-5">Voltar para meus pets</a>
        </app-empty-state>
      } @else if (failed()) {
        <div class="rounded-card bg-red-50 px-5 py-4 text-red-800" role="alert">
          Não foi possível carregar o pet. Tente novamente em instantes.
        </div>
      } @else {
        <div class="card mx-auto max-w-2xl p-6 sm:p-8">
          <h1 class="text-3xl font-bold text-pine-900">
            {{ petId ? 'Editar pet' : 'Cadastrar pet' }}
          </h1>

          <form class="mt-6 grid gap-4" [formGroup]="form" (ngSubmit)="submit()" novalidate>
            <div>
              <label class="field-label" for="p-name">Nome</label>
              <input id="p-name" class="field-input" formControlName="name" />
              <app-field-error [control]="form.controls.name" />
            </div>

            <div class="grid gap-4 sm:grid-cols-2">
              <div>
                <label class="field-label" for="p-species">Espécie</label>
                <select id="p-species" class="field-input" formControlName="species">
                  <option value="" disabled>Selecione</option>
                  @for (o of speciesOptions; track o.value) {
                    <option [value]="o.value">{{ o.label }}</option>
                  }
                </select>
                <app-field-error [control]="form.controls.species" />
              </div>
              <div>
                <label class="field-label" for="p-sex">Sexo</label>
                <select id="p-sex" class="field-input" formControlName="sex">
                  <option value="" disabled>Selecione</option>
                  @for (o of sexOptions; track o.value) {
                    <option [value]="o.value">{{ o.label }}</option>
                  }
                </select>
                <app-field-error [control]="form.controls.sex" />
              </div>
              <div>
                <label class="field-label" for="p-size">Porte</label>
                <select id="p-size" class="field-input" formControlName="size">
                  <option value="" disabled>Selecione</option>
                  @for (o of sizeOptions; track o.value) {
                    <option [value]="o.value">{{ o.label }}</option>
                  }
                </select>
                <app-field-error [control]="form.controls.size" />
              </div>
              <div>
                <label class="field-label" for="p-age">Faixa etária</label>
                <select id="p-age" class="field-input" formControlName="ageGroup">
                  <option value="" disabled>Selecione</option>
                  @for (o of ageOptions; track o.value) {
                    <option [value]="o.value">{{ o.label }}</option>
                  }
                </select>
                <app-field-error [control]="form.controls.ageGroup" />
              </div>
            </div>

            <div class="flex flex-wrap gap-6">
              <label class="flex items-center gap-2">
                <input type="checkbox" formControlName="isNeutered" /> Castrado
              </label>
              <label class="flex items-center gap-2">
                <input type="checkbox" formControlName="isVaccinated" /> Vacinado
              </label>
            </div>

            <div>
              <label class="field-label" for="p-description">Descrição</label>
              <textarea
                id="p-description"
                rows="6"
                class="field-input"
                formControlName="description"
              ></textarea>
              <app-field-error [control]="form.controls.description" />
            </div>

            <div>
              <label class="field-label" for="p-photo">Foto (JPEG, PNG ou WebP, até 5 MB)</label>
              <input
                id="p-photo"
                type="file"
                class="field-input"
                accept="image/jpeg,image/png,image/webp"
                (change)="onFileChange($event)"
              />
              @if (photoMessage(); as m) {
                <p class="field-error" role="alert" data-testid="photo-error">{{ m }}</p>
              }
              @if (previewUrl() ?? currentPhotoUrl(); as url) {
                <img
                  [src]="url"
                  alt="Pré-visualização da foto"
                  class="mt-3 max-h-60 rounded-card object-cover"
                />
              }
            </div>

            <button type="submit" class="btn btn-primary" [disabled]="submitting()">
              {{ submitting() ? 'Salvando...' : 'Salvar' }}
            </button>
          </form>
        </div>
      }
    </section>
  `,
})
export class PetFormPage {
  private readonly api = inject(ShelterApi);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly petId: string | null = inject(ActivatedRoute).snapshot.paramMap.get('id');

  protected readonly speciesOptions = options(SPECIES_LABELS);
  protected readonly sexOptions = options(SEX_LABELS);
  protected readonly sizeOptions = options(SIZE_LABELS);
  protected readonly ageOptions = options(AGE_GROUP_LABELS);

  protected readonly loading = signal(false);
  protected readonly notFound = signal(false);
  protected readonly failed = signal(false);
  protected readonly submitting = signal(false);
  protected readonly photoMessage = signal<string | null>(null);
  protected readonly previewUrl = signal<string | null>(null);
  protected readonly currentPhotoUrl = signal<string | null>(null);
  private photo: File | null = null;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [trimmedLength(2, 60)]],
    species: ['' as Species | '', [Validators.required]],
    sex: ['' as Sex | '', [Validators.required]],
    size: ['' as Size | '', [Validators.required]],
    ageGroup: ['' as AgeGroup | '', [Validators.required]],
    isNeutered: [false],
    isVaccinated: [false],
    description: ['', [trimmedLength(1, 2000)]],
  });

  constructor() {
    inject(DestroyRef).onDestroy(() => this.revokePreview());
    if (this.petId) {
      this.loading.set(true);
      this.api.getPet(this.petId).subscribe({
        next: (pet) => this.fill(pet),
        error: (err: HttpErrorResponse) => {
          if (err.status === 404) this.notFound.set(true);
          else this.failed.set(true);
          this.loading.set(false);
        },
      });
    }
  }

  protected onFileChange(event: Event): void {
    const element = event.target as HTMLInputElement;
    const file = element.files?.[0] ?? null;
    this.revokePreview();
    this.photo = null;
    this.photoMessage.set(null);
    if (!file) return;
    const error = photoError(file);
    if (error) {
      this.photoMessage.set(error);
      element.value = '';
      return;
    }
    this.photo = file;
    this.previewUrl.set(URL.createObjectURL(file));
  }

  protected submit(): void {
    if (this.submitting()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const input: PetInput = {
      ...raw,
      name: raw.name.trim(),
      description: raw.description.trim(),
      species: raw.species as Species,
      sex: raw.sex as Sex,
      size: raw.size as Size,
      ageGroup: raw.ageGroup as AgeGroup,
    };
    this.submitting.set(true);

    if (this.petId) {
      const id = this.petId;
      this.api.updatePet(id, input).subscribe({
        next: () => this.afterSave(id, false),
        error: (err: HttpErrorResponse) => this.fail(err),
      });
    } else {
      this.api.createPet(input).subscribe({
        next: ({ id }) => this.afterSave(id, true),
        error: (err: HttpErrorResponse) => this.fail(err),
      });
    }
  }

  private afterSave(id: string, created: boolean): void {
    const done = () => {
      this.toast.success(created ? 'Pet cadastrado!' : 'Pet atualizado!');
      void this.router.navigateByUrl('/ong/pets');
    };
    if (!this.photo) {
      done();
      return;
    }
    this.api.uploadPhoto(id, this.photo).subscribe({
      next: done,
      error: (err: HttpErrorResponse) => {
        this.toast.error(
          `O pet foi salvo, mas a foto não foi enviada. ${this.toast.problemMessage(err)}`,
        );
        this.submitting.set(false);
        if (created) void this.router.navigate(['/ong/pets', id, 'editar']);
      },
    });
  }

  private fail(err: HttpErrorResponse): void {
    this.submitting.set(false);
    this.toast.error(this.toast.problemMessage(err));
  }

  private fill(pet: ShelterPet): void {
    this.form.patchValue({
      name: pet.name,
      species: pet.species,
      sex: pet.sex,
      size: pet.size,
      ageGroup: pet.ageGroup,
      isNeutered: pet.isNeutered,
      isVaccinated: pet.isVaccinated,
      description: pet.description,
    });
    this.currentPhotoUrl.set(pet.photoUrl);
    this.loading.set(false);
  }

  private revokePreview(): void {
    const url = this.previewUrl();
    if (url) URL.revokeObjectURL(url);
    this.previewUrl.set(null);
  }
}
