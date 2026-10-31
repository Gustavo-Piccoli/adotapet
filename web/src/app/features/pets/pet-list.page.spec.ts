import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { PagedResult, PetSummary } from '../../core/api/api.models';
import { PetListPage } from './pet-list.page';

function pet(over: Partial<PetSummary> = {}): PetSummary {
  return {
    id: '1',
    name: 'Rex',
    species: 'Dog',
    sex: 'Male',
    size: 'Medium',
    ageGroup: 'Adult',
    photoUrl: null,
    city: 'Porto Alegre',
    state: 'RS',
    shelterName: 'ONG Patas',
    ...over,
  };
}

function paged(items: PetSummary[], page = 1, totalCount = items.length): PagedResult<PetSummary> {
  return { items, page, pageSize: 12, totalCount };
}

describe('PetListPage', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: '', component: PetListPage }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
  });

  afterEach(() => http.verify());

  const flushList = (body: PagedResult<PetSummary>) => {
    const req = http.expectOne((r) => r.url === '/api/pets');
    req.flush(body);
    return req.request;
  };

  it('renders cards with a placeholder when photoUrl is null', async () => {
    await harness.navigateByUrl('/');
    flushList(paged([pet()]));
    harness.detectChanges();

    const el: HTMLElement = harness.routeNativeElement!;
    expect(el.textContent).toContain('Rex');
    expect(el.textContent).toContain('Porto Alegre/RS');
    expect(el.querySelector('[data-testid="photo-placeholder"]')).not.toBeNull();
  });

  it('shows the empty state when there are no results', async () => {
    await harness.navigateByUrl('/');
    flushList(paged([]));
    harness.detectChanges();
    expect(harness.routeNativeElement!.textContent).toContain(
      'Nenhum pet encontrado com esses filtros.',
    );
  });

  it('calls the API with the filters and resets to page 1 when a filter changes', async () => {
    await harness.navigateByUrl('/?page=3&size=Large');
    const first = flushList(paged([pet()], 3, 40));
    expect(first.params.get('page')).toBe('3');
    expect(first.params.get('size')).toBe('Large');
    harness.detectChanges();

    const select = harness.routeNativeElement!.querySelector<HTMLSelectElement>('#f-species')!;
    select.value = 'Dog';
    select.dispatchEvent(new Event('change'));
    await harness.fixture.whenStable();

    const second = flushList(paged([pet()]));
    expect(second.params.get('species')).toBe('Dog');
    expect(second.params.get('size')).toBe('Large');
    expect(second.params.get('page')).toBe('1');
  });

  it('goes to the next page keeping filters', async () => {
    await harness.navigateByUrl('/?state=RS');
    flushList(paged([pet()], 1, 30));
    harness.detectChanges();

    const next = Array.from(harness.routeNativeElement!.querySelectorAll('button')).find(
      (b) => b.textContent?.trim() === 'Próxima',
    )!;
    next.click();
    await harness.fixture.whenStable();

    const req = flushList(paged([pet()], 2, 30));
    expect(req.params.get('page')).toBe('2');
    expect(req.params.get('state')).toBe('RS');
  });

  it('reflects deep-linked filters in the selects', async () => {
    await harness.navigateByUrl('/?species=Dog&state=RS&ageGroup=Puppy');
    flushList(paged([pet()]));
    harness.detectChanges();
    await harness.fixture.whenStable();

    const el: HTMLElement = harness.routeNativeElement!;
    expect(el.querySelector<HTMLSelectElement>('#f-species')!.value).toBe('Dog');
    expect(el.querySelector<HTMLSelectElement>('#f-state')!.value).toBe('RS');
    expect(el.querySelector<HTMLSelectElement>('#f-age')!.value).toBe('Puppy');
    expect(el.querySelector<HTMLSelectElement>('#f-size')!.value).toBe('');
  });

  it('retries from the error banner', async () => {
    await harness.navigateByUrl('/');
    http
      .expectOne((r) => r.url === '/api/pets')
      .flush({}, { status: 500, statusText: 'Server Error' });
    harness.detectChanges();
    const el: HTMLElement = harness.routeNativeElement!;
    expect(el.textContent).toContain('Não foi possível carregar os pets.');

    Array.from(el.querySelectorAll('button'))
      .find((b) => b.textContent?.trim() === 'Tentar novamente')!
      .click();
    harness.detectChanges();
    flushList(paged([pet()]));
    harness.detectChanges();
    expect(el.textContent).toContain('Rex');
    expect(el.textContent).not.toContain('Não foi possível carregar os pets.');
  });

  it('ignores prototype keys in the query string', async () => {
    await harness.navigateByUrl('/?species=toString&sex=constructor');
    const req = flushList(paged([pet()]));
    expect(req.params.has('species')).toBe(false);
    expect(req.params.has('sex')).toBe(false);
  });

  it('shows a status message while loading', async () => {
    await harness.navigateByUrl('/');
    harness.detectChanges();
    expect(harness.routeNativeElement!.querySelector('[role="status"]')?.textContent).toContain(
      'Carregando',
    );
    flushList(paged([pet()]));
  });

  it('redirects to the last valid page when the page is out of range', async () => {
    await harness.navigateByUrl('/?page=99');
    flushList(paged([], 99, 30));
    await harness.fixture.whenStable();

    const req = flushList(paged([pet()], 3, 30));
    expect(req.params.get('page')).toBe('3');
  });
});
