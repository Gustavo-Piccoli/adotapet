import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Injector,
  afterNextRender,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BehaviorSubject, Observable, catchError, combineLatest, map, of, switchMap, tap } from 'rxjs';
import { AdoptionRequestStatus, ShelterRequest } from '../../core/api/api.models';
import { ShelterApi } from '../../core/api/shelter.api';
import { ToastService } from '../../core/toast/toast.service';
import { ConfirmDialog } from '../../shared/confirm-dialog';
import { ErrorBanner } from '../../shared/error-banner';
import { EmptyState } from '../../shared/empty-state';
import { formatDate } from '../../shared/format';
import { REQUEST_STATUS_LABELS } from '../../shared/labels';
import { formatPhone } from '../../shared/validators';

export const APPROVE_CONFIRM_MESSAGE =
  'Aprovar este pedido marca o pet como adotado e recusa os outros pedidos pendentes. Continuar?';

export const rejectConfirmMessage = (r: ShelterRequest): string =>
  `Recusar o pedido de ${r.adopterName} para ${r.petName}? Essa ação não pode ser desfeita.`;

const STATUSES: AdoptionRequestStatus[] = ['Pending', 'Approved', 'Rejected'];

@Component({
  selector: 'app-shelter-requests-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, EmptyState, ConfirmDialog, ErrorBanner],
  template: `
    <section class="container-page py-8">
      <h1 #heading tabindex="-1" class="text-3xl font-bold text-pine-900">Pedidos de adoção</h1>

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
        <p class="sr-only" role="status">Carregando…</p>
        <div class="card mt-6 h-32 animate-pulse bg-cream-200" aria-hidden="true"></div>
      } @else if (failed()) {
        <app-error-banner
          class="mt-6 block"
          message="Não foi possível carregar os pedidos. Tente novamente em instantes."
          (retry)="reload$.next()"
        />
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
                  [class.chip-success]="r.status === 'Approved'"
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
                  <dd>{{ phone(r.adopterPhone) }}</dd>
                </div>
              </dl>
              <p class="mt-3 whitespace-pre-line">{{ r.message }}</p>
              @if (r.status === 'Pending') {
                <div class="mt-4 flex gap-2">
                  <button
                    type="button"
                    class="btn btn-primary"
                    [disabled]="busy()"
                    (click)="confirming.set({ action: 'approve', request: r })"
                  >
                    Aprovar
                  </button>
                  <button
                    type="button"
                    class="btn btn-danger"
                    [disabled]="busy()"
                    (click)="confirming.set({ action: 'reject', request: r })"
                  >
                    Recusar
                  </button>
                </div>
              }
            </li>
          }
        </ul>
      }

      @if (confirming(); as c) {
        <app-confirm-dialog
          [message]="c.action === 'approve' ? approveMessage : rejectMessage(c.request)"
          (confirmed)="c.action === 'approve' ? approve(c.request) : reject(c.request)"
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
  protected readonly phone = formatPhone;
  protected readonly approveMessage = APPROVE_CONFIRM_MESSAGE;
  protected readonly rejectMessage = rejectConfirmMessage;

  protected readonly requests = signal<ShelterRequest[]>([]);
  protected readonly loading = signal(true);
  protected readonly failed = signal(false);
  protected readonly busy = signal(false);
  protected readonly confirming = signal<{
    action: 'approve' | 'reject';
    request: ShelterRequest;
  } | null>(null);

  private readonly heading = viewChild.required<ElementRef<HTMLElement>>('heading');
  private readonly injector = inject(Injector);
  /** The decided request's buttons disappear on reload, so focus moves to the heading afterwards. */
  private refocusAfterLoad = false;

  private readonly status$: Observable<AdoptionRequestStatus> = this.route.queryParamMap.pipe(
    map((p) => {
      const v = p.get('status') as AdoptionRequestStatus | null;
      return v && STATUSES.includes(v) ? v : 'Pending';
    }),
  );
  protected readonly status = toSignal(this.status$, { initialValue: 'Pending' });

  protected readonly reload$ = new BehaviorSubject<void>(undefined);

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
        if (this.refocusAfterLoad) {
          this.refocusAfterLoad = false;
          afterNextRender(() => this.heading().nativeElement.focus(), { injector: this.injector });
        }
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
    this.confirming.set(null);
    this.decide(this.api.rejectRequest(r.id), 'Pedido recusado.');
  }

  private decide(call: Observable<void>, successMessage: string): void {
    if (this.busy()) return;
    this.busy.set(true);
    this.refocusAfterLoad = true;
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
        else this.refocusAfterLoad = false;
      },
    });
  }
}
