import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, of, switchMap, tap } from 'rxjs';
import { PagedResult, PetSummary, SearchPetsParams } from '../../core/api/api.models';
import { PetsApi } from '../../core/api/pets.api';
import { EmptyState } from '../../shared/empty-state';
import {
  AGE_GROUP_LABELS,
  SEX_LABELS,
  SIZE_LABELS,
  SPECIES_LABELS,
  UFS,
} from '../../shared/labels';
import { Paginator } from './paginator';
import { PetCard } from './pet-card';
import { PetFilterValues, PetFilters } from './pet-filters';

const PAGE_SIZE = 12;

function pick<T extends string>(raw: string | null, allowed: Record<T, string>): T | undefined {
  return raw !== null && raw in allowed ? (raw as T) : undefined;
}

@Component({
  selector: 'app-pet-list-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PetFilters, PetCard, Paginator, EmptyState],
  template: `
    <section class="bg-pine-900 pb-14 text-white">
      <div class="container-page py-12 sm:py-16">
        <p class="chip chip-honey mb-4">Adoção responsável</p>
        <h1 class="max-w-2xl text-4xl font-bold leading-tight sm:text-5xl">
          Um novo <span class="text-honey-500">melhor amigo</span> está esperando por você.
        </h1>
        <p class="mt-4 max-w-xl text-lg text-pine-100">
          Conheça cães e gatos de ONGs de proteção animal e envie seu pedido de adoção em poucos
          cliques.
        </p>
      </div>
    </section>

    <div class="container-page relative -mt-8">
      <app-pet-filters [value]="filters()" (valueChange)="onFiltersChange($event)" />
    </div>

    <section class="container-page mt-10" aria-labelledby="titulo-lista" [attr.aria-busy]="loading()">
      <h2 id="titulo-lista" class="mb-5 text-2xl font-bold text-pine-900">Pets para adoção</h2>

      @if (error()) {
        <div class="rounded-card bg-red-50 px-5 py-4 text-red-800" role="alert">
          Não foi possível carregar os pets. Tente novamente em instantes.
        </div>
      } @else if (!result()) {
        <div class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4" aria-hidden="true">
          @for (n of skeletons; track n) {
            <div class="card h-72 animate-pulse bg-cream-200"></div>
          }
        </div>
      } @else if (result(); as r) {
        @if (r.items.length === 0) {
          <app-empty-state message="Nenhum pet encontrado com esses filtros." />
        } @else {
          <ul class="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            @for (pet of r.items; track pet.id) {
              <li><app-pet-card [pet]="pet" /></li>
            }
          </ul>
          <app-paginator
            [page]="page()"
            [pageSize]="pageSize"
            [totalCount]="r.totalCount"
            (pageChange)="onPageChange($event)"
          />
        }
      }
    </section>
  `,
})
export class PetListPage {
  private readonly api = inject(PetsApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly pageSize = PAGE_SIZE;
  protected readonly skeletons = Array.from({ length: 8 }, (_, i) => i);

  private readonly queryParams = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap,
  });

  protected readonly filters = computed<PetFilterValues>(() => {
    const q = this.queryParams();
    const state = q.get('state');
    return {
      species: pick(q.get('species'), SPECIES_LABELS),
      sex: pick(q.get('sex'), SEX_LABELS),
      size: pick(q.get('size'), SIZE_LABELS),
      ageGroup: pick(q.get('ageGroup'), AGE_GROUP_LABELS),
      state: state !== null && (UFS as readonly string[]).includes(state) ? state : undefined,
      city: q.get('city')?.trim() || undefined,
    };
  });

  protected readonly page = computed(() => {
    const n = Number(this.queryParams().get('page'));
    return Number.isInteger(n) && n >= 1 ? n : 1;
  });

  protected readonly loading = signal(true);
  protected readonly error = signal(false);

  protected readonly result = toSignal<PagedResult<PetSummary> | null>(
    toObservable(
      computed<SearchPetsParams>(() => ({
        ...this.filters(),
        page: this.page(),
        pageSize: PAGE_SIZE,
      })),
    ).pipe(
      tap(() => {
        this.loading.set(true);
        this.error.set(false);
      }),
      switchMap((params) =>
        this.api.search(params).pipe(
          catchError(() => {
            this.error.set(true);
            return of(null);
          }),
        ),
      ),
      tap((r) => {
        this.loading.set(false);
        this.redirectIfPastLastPage(r);
      }),
    ),
    { initialValue: null },
  );

  private redirectIfPastLastPage(r: PagedResult<PetSummary> | null): void {
    if (r && r.items.length === 0 && r.totalCount > 0 && this.page() > 1) {
      const lastPage = Math.ceil(r.totalCount / PAGE_SIZE);
      void this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { page: lastPage === 1 ? null : lastPage },
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    }
  }

  protected onFiltersChange(next: PetFilterValues): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { ...next, page: null },
    });
  }

  protected onPageChange(page: number): void {
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: page === 1 ? null : page },
      queryParamsHandling: 'merge',
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
