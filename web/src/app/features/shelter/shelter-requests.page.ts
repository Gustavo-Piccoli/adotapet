import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BehaviorSubject, Observable, catchError, combineLatest, map, of, switchMap, tap } from 'rxjs';
import { AdoptionRequestStatus, ShelterRequest } from '../../core/api/api.models';
import { ShelterApi } from '../../core/api/shelter.api';
import { ToastService } from '../../core/toast/toast.service';
import { ConfirmDialog } from '../../shared/confirm-dialog';
import { EmptyState } from '../../shared/empty-state';
import { formatDate } from '../../shared/format';
import { REQUEST_STATUS_LABELS } from '../../shared/labels';

export const APPROVE_CONFIRM_MESSAGE =
  'Aprovar este pedido marca o pet como adotado e recusa os outros pedidos pendentes. Continuar?';

const STATUSES: AdoptionRequestStatus[] = ['Pending', 'Approved', 'Rejected'];

@Component({
  selector: 'app-shelter-requests-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, EmptyState, ConfirmDialog],
  template: `
    <section class="container-page py-8">
      <h1 class="text-3xl font-bold text-pine-900">Pedidos de adoção</h1>

      <div class="mt-4 flex flex-wrap gap-2" role="group" aria-label="Filtrar por status">
        @for (s of statuses; track s) {
          <button
            type="button"
            class="btn"
            [class.btn-primary]="s === status()"
            [class.btn-secondary]="s !== status()"
            [attr.aria-pressed]="s === status()"
            (click)="setStatus(s)"
          >
            {{ labels[s] }}
          </button>
        }
      </div>

      @if (loading()) {
        <div class="card mt-6 h-32 animate-pulse bg-cream-200" aria-hidden="true"></div>
      } @else if (failed()) {
        <div class="mt-6 rounded-card bg-red-50 px-5 py-4 text-red-800" role="alert">
          Não foi possível carregar os pedidos. Tente novamente em instantes.
        </div>
      } @else if (requests().length === 0) {
        <div class="mt-6">
          <app-empty-state message="Nenhum pedido com este status." />
        </div>
      } @else {
        <ul class="mt-6 grid gap-4">
          @for (r of requests(); track r.id) {
            <li class="card p-5" data-testid="shelter-request">
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
                  >{{ labels[r.status] }}</span
                >
                <span class="text-sm text-ink-soft">{{ date(r.createdAt) }}</span>
              </div>
              <dl class="mt-3 grid gap-1 text-sm">
                <div class="flex gap-2">
                  <dt class="font-semibold">Adotante:</dt>
                  <dd>{{ r.adopterName }} ({{ r.adopterCity }}/{{ r.adopterState }})</dd>
                </div>
                <div class="flex gap-2">
                  <dt class="font-semibold">E-mail:</dt>
                  <dd>
                    <a class="text-clay-700 underline" [href]="'mailto:' + r.adopterEmail">{{
                      r.adopterEmail
                    }}</a>
                  </dd>
                </div>
                <div class="flex gap-2">
                  <dt class="font-semibold">Telefone:</dt>
                  <dd>{{ r.adopterPhone }}</dd>
                </div>
              </dl>
              <p class="mt-3 whitespace-pre-line">{{ r.message }}</p>
              @if (r.status === 'Pending') {
                <div class="mt-4 flex gap-2">
                  <button
                    type="button"
                    class="btn btn-primary"
                    [disabled]="busy()"
                    (click)="confirming.set(r)"
                  >
                    Aprovar
                  </button>
                  <button
                    type="button"
                    class="btn btn-danger"
                    [disabled]="busy()"
                    (click)="reject(r)"
                  >
                    Recusar
                  </button>
                </div>
              }
            </li>
          }
        </ul>
      }

      @if (confirming(); as r) {
        <app-confirm-dialog
          [message]="approveMessage"
          (confirmed)="approve(r)"
          (cancelled)="confirming.set(null)"
        />
      }
    </section>
  `,
})
export class ShelterRequestsPage {
  private readonly api = inject(ShelterApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly statuses = STATUSES;
  protected readonly labels = REQUEST_STATUS_LABELS;
  protected readonly date = formatDate;
  protected readonly approveMessage = APPROVE_CONFIRM_MESSAGE;

  protected readonly requests = signal<ShelterRequest[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  protected readonly busy = signal(false);
  protected readonly confirming = signal<ShelterRequest | null>(null);

  private readonly status$: Observable<AdoptionRequestStatus> = this.route.queryParamMap.pipe(
    map((p) => {
      const v = p.get('status') as AdoptionRequestStatus | null;
      return v && STATUSES.includes(v) ? v : 'Pending';
    }),
  );
  protected readonly status = toSignal(this.status$, { initialValue: 'Pending' });

  private readonly reload$ = new BehaviorSubject<void>(undefined);

  constructor() {
    combineLatest([this.status$, this.reload$])
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.failed.set(false);
        }),
        switchMap(([status]) =>
          this.api.listRequests(status).pipe(
            catchError(() => {
              this.failed.set(true);
              return of([] as ShelterRequest[]);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((list) => {
        this.requests.set(list);
        this.loading.set(false);
      });
  }

  protected setStatus(status: AdoptionRequestStatus): void {
    void this.router.navigate([], { queryParams: { status }, queryParamsHandling: 'merge' });
  }

  protected approve(r: ShelterRequest): void {
    this.confirming.set(null);
    this.decide(this.api.approveRequest(r.id), 'Pedido aprovado!');
  }

  protected reject(r: ShelterRequest): void {
    this.decide(this.api.rejectRequest(r.id), 'Pedido recusado.');
  }

  private decide(call: Observable<void>, successMessage: string): void {
    this.busy.set(true);
    call.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(successMessage);
        this.reload$.next();
      },
      error: (err: HttpErrorResponse) => {
        this.busy.set(false);
        this.toast.error(this.toast.problemMessage(err));
        if (err.status === 409) this.reload$.next();
      },
    });
  }
}
