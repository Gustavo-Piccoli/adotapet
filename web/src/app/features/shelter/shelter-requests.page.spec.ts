import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { ShelterRequest } from '../../core/api/api.models';
import { ToastService } from '../../core/toast/toast.service';
import { APPROVE_CONFIRM_MESSAGE, ShelterRequestsPage } from './shelter-requests.page';

const REQUEST: ShelterRequest = {
  id: 'r1',
  petId: 'p1',
  petName: 'Rex',
  adopterName: 'Ana',
  adopterEmail: 'ana@x.com',
  adopterPhone: '51999999999',
  adopterCity: 'Porto Alegre',
  adopterState: 'RS',
  message: 'Quero adotar',
  status: 'Pending',
  createdAt: '2026-01-02T12:00:00Z',
  decidedAt: null,
};

describe('ShelterRequestsPage', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;
  let el: HTMLElement;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'ong/pedidos', component: ShelterRequestsPage }]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/ong/pedidos');
    el = harness.routeNativeElement as HTMLElement;
  });

  afterEach(() => http.verify());

  const listReq = (status: string) =>
    http.expectOne((r) => r.url === '/api/shelter/adoption-requests' && r.params.get('status') === status);

  const button = (text: string) =>
    Array.from(el.querySelectorAll('button')).find((b) => b.textContent?.trim() === text)!;

  it('loads Pending by default and shows contact and message', async () => {
    listReq('Pending').flush([REQUEST]);
    harness.detectChanges();
    expect(el.textContent).toContain('ana@x.com');
    expect(el.textContent).toContain('51999999999');
    expect(el.textContent).toContain('Quero adotar');
    expect(el.textContent).toContain('02/01/2026');
  });

  it('approving asks for confirmation, calls the API, then reloads the list', async () => {
    listReq('Pending').flush([REQUEST]);
    harness.detectChanges();

    button('Aprovar').click();
    harness.detectChanges();
    expect(el.textContent).toContain(APPROVE_CONFIRM_MESSAGE);
    http.expectNone('/api/shelter/adoption-requests/r1/approve');

    button('Continuar').click();
    http.expectOne('/api/shelter/adoption-requests/r1/approve').flush(null);
    listReq('Pending').flush([]);
    harness.detectChanges();
    expect(el.textContent).toContain('Nenhum pedido com este status.');
  });

  it('cancelling the confirmation does not call the API', () => {
    listReq('Pending').flush([REQUEST]);
    harness.detectChanges();
    button('Aprovar').click();
    harness.detectChanges();
    button('Cancelar').click();
    harness.detectChanges();
    expect(el.textContent).not.toContain(APPROVE_CONFIRM_MESSAGE);
  });

  it('on 409 shows the API detail in a toast and reloads', () => {
    listReq('Pending').flush([REQUEST]);
    harness.detectChanges();
    button('Recusar').click();
    http
      .expectOne('/api/shelter/adoption-requests/r1/reject')
      .flush({ detail: 'Pedido já decidido.' }, { status: 409, statusText: 'Conflict' });
    expect(TestBed.inject(ToastService).toasts()[0].message).toBe('Pedido já decidido.');
    listReq('Pending').flush([]);
  });

  it('changing the filter reloads with the chosen status', async () => {
    listReq('Pending').flush([REQUEST]);
    harness.detectChanges();
    button('Aprovado').click();
    await harness.fixture.whenStable();
    listReq('Approved').flush([]);
  });
});
