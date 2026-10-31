import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BehaviorSubject, catchError, combineLatest, concat, map, of, switchMap, tap } from 'rxjs';
import { PetDetails } from '../../core/api/api.models';
import { AdoptionRequestsApi } from '../../core/api/adoption-requests.api';
import { PetsApi } from '../../core/api/pets.api';
import { AuthService } from '../../core/auth/auth.service';
import { RouteAnnouncer } from '../../core/title/route-announcer';
import { ProblemDetails } from '../../core/http/problem-details';
import { ToastService } from '../../core/toast/toast.service';
import { ErrorBanner } from '../../shared/error-banner';
import { EmptyState } from '../../shared/empty-state';
import { FieldA11y, FieldError } from '../../shared/field-error';
import { AGE_GROUP_LABELS, SEX_LABELS, SIZE_LABELS, SPECIES_LABELS } from '../../shared/labels';
import { PawIcon } from '../../shared/paw-icon';
import { formatPhone } from '../../shared/validators';

@Component({
  selector: 'app-pet-details-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, PawIcon, EmptyState, ErrorBanner, FieldError, FieldA11y],
  template: `
    <div class="container-page py-8">
      <a routerLink="/" class="btn btn-ghost mb-4 -ml-3">&larr; Voltar para os pets</a>

      @if (loading()) {
        <p class="sr-only" role="status">Carregando…</p>
        <div class="card h-96 animate-pulse bg-cream-200" aria-hidden="true"></div>
      } @else if (notFound()) {
        <app-empty-state message="Pet não encontrado.">
          <a routerLink="/" class="btn btn-primary mt-5">Ver todos os pets</a>
        </app-empty-state>
      } @else if (failed()) {
        <app-error-banner
          message="Não foi possível carregar o pet. Tente novamente em instantes."
          (retry)="reload$.next()"
        />
      } @else if (pet(); as p) {
        <article class="grid gap-8 lg:grid-cols-2">
          <div class="card aspect-4/3 bg-clay-50">
            @if (p.photoUrl; as url) {
              <img [src]="url" [alt]="'Foto de ' + p.name" class="size-full object-cover" />
            } @else {
              <div class="grid size-full place-items-center text-clay-500" data-testid="photo-placeholder">
                <app-paw-icon class="size-28 opacity-60" />
                <span class="sr-only">Sem foto</span>
              </div>
            }
          </div>

          <div>
            <div class="flex flex-wrap items-center gap-3">
              <h1 class="text-4xl font-bold text-pine-900">{{ p.name }}</h1>
              @if (p.status === 'Adopted') {
                <span class="chip chip-clay" data-testid="adopted-badge">Adotado</span>
              }
            </div>

            <div class="mt-3 flex flex-wrap gap-1.5">
              <span class="chip chip-honey">{{ species[p.species] }}</span>
              <span class="chip">{{ sex[p.sex] }}</span>
              <span class="chip">{{ size[p.size] }}</span>
              <span class="chip">{{ age[p.ageGroup] }}</span>
              <span class="chip" [class.chip-clay]="!p.isNeutered">
                {{ p.isNeutered ? 'Castrado' : 'Não castrado' }}
              </span>
              <span class="chip" [class.chip-clay]="!p.isVaccinated">
                {{ p.isVaccinated ? 'Vacinado' : 'Não vacinado' }}
              </span>
            </div>

            <p class="mt-5 whitespace-pre-line text-ink">{{ p.description }}</p>

            @if (p.status === 'Available' && !auth.isShelter()) {
              <div class="mt-6">
                @if (pending()) {
                  <p class="chip chip-honey px-4 py-2 text-sm" data-testid="pending-badge" role="status">
                    Pedido em análise
                  </p>
                } @else if (!auth.isLoggedIn()) {
                  <button type="button" class="btn btn-primary" (click)="goToLogin(p)">
                    Quero adotar
                  </button>
                } @else if (!pendingChecked()) {
                  <div class="h-11 w-40 animate-pulse rounded-xl bg-cream-200" aria-hidden="true"></div>
                } @else if (!formOpen()) {
                  <button type="button" class="btn btn-primary" (click)="formOpen.set(true)">
                    Quero adotar
                  </button>
                } @else {
                  <form
                    class="card grid gap-3 p-5"
                    [formGroup]="form"
                    (ngSubmit)="send(p)"
                    novalidate
                  >
                    <div>
                      <label class="field-label" for="req-message">
                        Conte por que você quer adotar {{ p.name }}
                      </label>
                      <textarea
                        id="req-message"
                        rows="5"
                        class="field-input"
                        formControlName="message"
                      ></textarea>
                      <app-field-error [control]="form.controls.message" />
                    </div>
                    <div class="flex gap-2">
                      <button type="submit" class="btn btn-primary" [disabled]="sending()">
                        {{ sending() ? 'Enviando...' : 'Enviar pedido' }}
                      </button>
                      <button type="button" class="btn btn-ghost" (click)="formOpen.set(false)">
                        Cancelar
                      </button>
                    </div>
                  </form>
                }
              </div>
            }

            <section class="card mt-8 p-5" aria-labelledby="ong-titulo">
              <h2 id="ong-titulo" class="text-xl font-bold text-pine-900">{{ p.shelter.name }}</h2>
              <p class="text-sm text-ink-soft">{{ p.shelter.city }}/{{ p.shelter.state }}</p>
              @if (p.shelter.description) {
                <p class="mt-3 whitespace-pre-line text-sm">{{ p.shelter.description }}</p>
              }
              <dl class="mt-3 grid gap-1 text-sm">
                <div class="flex gap-2">
                  <dt class="font-semibold">Telefone:</dt>
                  <dd>{{ phone(p.shelter.phone) }}</dd>
                </div>
                <div class="flex gap-2">
                  <dt class="font-semibold">E-mail:</dt>
                  <dd>
                    <a class="text-clay-700 underline" [href]="'mailto:' + p.shelter.email">{{
                      p.shelter.email
                    }}</a>
                  </dd>
                </div>
              </dl>
            </section>
          </div>
        </article>
      }
    </div>
  `,
})
export class PetDetailsPage {
  protected readonly auth = inject(AuthService);
  private readonly petsApi = inject(PetsApi);
  private readonly requestsApi = inject(AdoptionRequestsApi);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly title = inject(Title);
  private readonly announcer = inject(RouteAnnouncer);
  protected readonly reload$ = new BehaviorSubject<void>(undefined);

  protected readonly phone = formatPhone;
  protected readonly species = SPECIES_LABELS;
  protected readonly sex = SEX_LABELS;
  protected readonly size = SIZE_LABELS;
  protected readonly age = AGE_GROUP_LABELS;

  protected readonly pet = signal<PetDetails | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly failed = signal(false);
  protected readonly pending = signal(false);
  protected readonly pendingChecked = signal(false);
  protected readonly formOpen = signal(false);
  protected readonly sending = signal(false);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    message: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]],
  });

  constructor() {
    combineLatest([inject(ActivatedRoute).paramMap, this.reload$])
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.notFound.set(false);
          this.failed.set(false);
          this.pending.set(false);
          this.pendingChecked.set(false);
          this.formOpen.set(false);
          this.form.reset();
        }),
        switchMap(([params]) =>
          this.petsApi.get(params.get('id') ?? '').pipe(
            catchError((err: HttpErrorResponse) => {
              if (err.status === 404) this.notFound.set(true);
              else this.failed.set(true);
              return of(null);
            }),
            // Chained here so a result for a previous pet is cancelled by switchMap.
            switchMap((pet) =>
              pet && this.auth.isAdopter()
                ? concat(
                    of({ pet, pending: false, checked: false, first: true }),
                    this.requestsApi.listMine().pipe(
                      map((list) => list.some((r) => r.petId === pet.id && r.status === 'Pending')),
                      catchError(() => of(false)),
                      map((pending) => ({ pet, pending, checked: true, first: false })),
                    ),
                  )
                : of({ pet, pending: false, checked: true, first: true }),
            ),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe(({ pet, pending, checked, first }) => {
        this.pet.set(pet);
        this.loading.set(false);
        this.pending.set(pending);
        this.pendingChecked.set(checked);
        if (!first) return;
        if (pet) this.title.setTitle(`${pet.name} — AdotPet`);
        else if (this.notFound()) this.title.setTitle('Pet não encontrado — AdotPet');
        else this.title.setTitle('Erro ao carregar o pet — AdotPet');
        this.announcer.announce(this.title.getTitle());
      });
  }

  protected goToLogin(pet: PetDetails): void {
    void this.router.navigate(['/entrar'], { queryParams: { returnUrl: `/pets/${pet.id}` } });
  }

  protected send(pet: PetDetails): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.sending.set(true);
    this.petsApi.createAdoptionRequest(pet.id, this.form.getRawValue().message.trim()).subscribe({
      next: () => {
        this.sending.set(false);
        this.pending.set(true);
        this.toast.success('Pedido enviado! A ONG vai analisar.');
      },
      error: (err: HttpErrorResponse) => {
        this.sending.set(false);
        const code = err.status === 409 ? (err.error as ProblemDetails | null)?.code : undefined;
        if (code === 'duplicate_pending_request') {
          this.pending.set(true);
        } else {
          // Anything else, including pet_unavailable: tell the user why. If the pet was adopted
          // meanwhile, reload it so the page shows "Adotado" and hides the adopt button.
          this.toast.error(this.toast.problemMessage(err));
          if (code === 'pet_unavailable') this.reload$.next();
        }
      },
    });
  }
}
