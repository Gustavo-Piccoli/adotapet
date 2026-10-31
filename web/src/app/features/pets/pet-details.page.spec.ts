import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AdopterRequest, PetDetails } from '../../core/api/api.models';
import { AuthResult, Role } from '../../core/auth/auth.models';
import { AuthService } from '../../core/auth/auth.service';
import { RouteAnnouncer } from '../../core/title/route-announcer';
import { ToastService } from '../../core/toast/toast.service';
import { PetDetailsPage } from './pet-details.page';

const PET: PetDetails = {
  id: 'p1',
  name: 'Rex',
  species: 'Dog',
  sex: 'Male',
  size: 'Medium',
  ageGroup: 'Adult',
  isNeutered: true,
  isVaccinated: false,
  description: 'Muito dócil.',
  photoUrl: null,
  status: 'Available',
  shelter: {
    id: 's1',
    name: 'ONG Patas',
    city: 'Porto Alegre',
    state: 'RS',
    phone: '51999998888',
    email: 'ong@patas.org',
    description: '',
  },
};

const MESSAGE = 'Tenho quintal e muito amor para dar.';

describe('PetDetailsPage', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;

  beforeEach(async () => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'pets/:id', component: PetDetailsPage },
          { path: 'entrar', component: PetDetailsPage },
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
  });

  afterEach(() => http.verify());

  const login = (role: Role) =>
    TestBed.inject(AuthService).session.set({
      accessToken: 't',
      expiresAt: '2999-01-01T00:00:00Z',
      role,
      profileId: 'x',
      name: 'N',
      email: 'n@x.com',
    } satisfies AuthResult);

  const open = async (pet: PetDetails = PET, mine: AdopterRequest[] = []) => {
    await harness.navigateByUrl('/pets/p1');
    http.expectOne('/api/pets/p1').flush(pet);
    if (TestBed.inject(AuthService).isAdopter()) {
      http.expectOne('/api/adopter/adoption-requests').flush(mine);
    }
    harness.detectChanges();
    return harness.routeNativeElement as HTMLElement;
  };

  const button = (el: HTMLElement, text: string) =>
    Array.from(el.querySelectorAll('button')).find((b) => b.textContent?.trim() === text);

  const fillAndSend = (el: HTMLElement) => {
    const textarea = el.querySelector<HTMLTextAreaElement>('#req-message')!;
    textarea.value = MESSAGE;
    textarea.dispatchEvent(new Event('input'));
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
  };

  it('renders details, badges and the shelter card', async () => {
    const el = await open();
    expect(el.textContent).toContain('Rex');
    expect(el.textContent).toContain('Castrado');
    expect(el.textContent).toContain('Não vacinado');
    expect(el.textContent).toContain('Muito dócil.');
    expect(el.textContent).toContain('ONG Patas');
    expect(el.textContent).toContain('Porto Alegre/RS');
    expect(el.textContent).toContain('ong@patas.org');
    expect(el.textContent).toContain('(51) 99999-8888');
    expect(el.querySelector('[data-testid="photo-placeholder"]')).not.toBeNull();
  });

  it('sends a visitor to login with the returnUrl', async () => {
    const el = await open();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate');
    button(el, 'Quero adotar')!.click();
    expect(navigate).toHaveBeenCalledWith(['/entrar'], {
      queryParams: { returnUrl: '/pets/p1' },
    });
  });

  it('hides the adopt button for shelters', async () => {
    login('Shelter');
    const el = await open();
    expect(button(el, 'Quero adotar')).toBeUndefined();
  });

  it('shows the Adopted badge and no button for adopted pets', async () => {
    login('Adopter');
    const el = await open({ ...PET, status: 'Adopted' });
    expect(el.querySelector('[data-testid="adopted-badge"]')?.textContent).toContain('Adotado');
    expect(button(el, 'Quero adotar')).toBeUndefined();
  });

  it('shows "Pedido em análise" when a pending request already exists', async () => {
    login('Adopter');
    const el = await open(PET, [
      { petId: 'p1', status: 'Pending' } as AdopterRequest,
    ]);
    expect(el.querySelector('[data-testid="pending-badge"]')?.textContent).toContain(
      'Pedido em análise',
    );
    expect(button(el, 'Quero adotar')).toBeUndefined();
  });

  it('does not offer adoption to an adopter until the pending check resolves', async () => {
    login('Adopter');
    await harness.navigateByUrl('/pets/p1');
    http.expectOne('/api/pets/p1').flush(PET);
    const checkReq = http.expectOne('/api/adopter/adoption-requests');
    harness.detectChanges();
    const el = harness.routeNativeElement as HTMLElement;
    expect(el.textContent).toContain('Rex');
    expect(button(el, 'Quero adotar')).toBeUndefined();
    expect(el.querySelector('[data-testid="pending-badge"]')).toBeNull();

    checkReq.flush([{ petId: 'p1', status: 'Pending' } as AdopterRequest]);
    harness.detectChanges();
    expect(el.querySelector('[data-testid="pending-badge"]')).not.toBeNull();
    expect(button(el, 'Quero adotar')).toBeUndefined();
  });

  it('offers adoption once the pending check resolves without a pending request', async () => {
    login('Adopter');
    await harness.navigateByUrl('/pets/p1');
    http.expectOne('/api/pets/p1').flush(PET);
    http.expectOne('/api/adopter/adoption-requests').flush([]);
    harness.detectChanges();
    expect(button(harness.routeNativeElement as HTMLElement, 'Quero adotar')).toBeDefined();
  });

  it('drops the pending check of a previous pet when navigating to another', async () => {
    login('Adopter');
    await harness.navigateByUrl('/pets/p1');
    http.expectOne('/api/pets/p1').flush(PET);
    const stale = http.expectOne('/api/adopter/adoption-requests');
    await harness.navigateByUrl('/pets/p2');
    expect(stale.cancelled).toBe(true);
    http.expectOne('/api/pets/p2').flush({ ...PET, id: 'p2', name: 'Mel' });
    http.expectOne('/api/adopter/adoption-requests').flush([]);
    harness.detectChanges();
    expect(button(harness.routeNativeElement as HTMLElement, 'Quero adotar')).toBeDefined();
  });

  it('validates the message length before posting', async () => {
    login('Adopter');
    const el = await open();
    button(el, 'Quero adotar')!.click();
    harness.detectChanges();
    const textarea = el.querySelector<HTMLTextAreaElement>('#req-message')!;
    textarea.value = 'curta';
    textarea.dispatchEvent(new Event('input'));
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
    harness.detectChanges();
    expect(el.textContent).toContain('no mínimo 10');
    http.expectNone('/api/pets/p1/adoption-requests');
  });

  it('posts the request and confirms with a toast', async () => {
    login('Adopter');
    const el = await open();
    button(el, 'Quero adotar')!.click();
    harness.detectChanges();
    fillAndSend(el);

    const req = http.expectOne('/api/pets/p1/adoption-requests');
    expect(req.request.body).toEqual({ message: MESSAGE });
    req.flush({ id: 'r1' }, { status: 201, statusText: 'Created' });
    harness.detectChanges();

    expect(TestBed.inject(ToastService).toasts()[0].message).toBe(
      'Pedido enviado! A ONG vai analisar.',
    );
    expect(el.querySelector('[data-testid="pending-badge"]')).not.toBeNull();
  });

  it('shows "Pedido em análise" on a 409 instead of an error', async () => {
    login('Adopter');
    const el = await open();
    button(el, 'Quero adotar')!.click();
    harness.detectChanges();
    fillAndSend(el);

    http
      .expectOne('/api/pets/p1/adoption-requests')
      .flush(
        { detail: 'Você já tem um pedido.', code: 'duplicate_pending_request' },
        { status: 409, statusText: 'Conflict' },
      );
    harness.detectChanges();

    expect(el.querySelector('[data-testid="pending-badge"]')?.textContent).toContain(
      'Pedido em análise',
    );
    expect(TestBed.inject(ToastService).toasts()).toHaveLength(0);
  });

  it('reloads the pet and toasts the detail when it became unavailable (409 pet_unavailable)', async () => {
    login('Adopter');
    const el = await open();
    button(el, 'Quero adotar')!.click();
    harness.detectChanges();
    fillAndSend(el);

    http.expectOne('/api/pets/p1/adoption-requests').flush(
      { detail: 'Este pet não está mais disponível para adoção.', code: 'pet_unavailable' },
      { status: 409, statusText: 'Conflict' },
    );
    http.expectOne('/api/pets/p1').flush({ ...PET, status: 'Adopted' });
    http.expectOne('/api/adopter/adoption-requests').flush([]);
    harness.detectChanges();

    expect(el.querySelector('[data-testid="pending-badge"]')).toBeNull();
    expect(el.querySelector('[data-testid="adopted-badge"]')).not.toBeNull();
    expect(button(el, 'Quero adotar')).toBeUndefined();
    expect(TestBed.inject(ToastService).toasts()[0].message).toBe(
      'Este pet não está mais disponível para adoção.',
    );
  });

  it('only toasts the detail on a 409 with an unknown code', async () => {
    login('Adopter');
    const el = await open();
    button(el, 'Quero adotar')!.click();
    harness.detectChanges();
    fillAndSend(el);

    http
      .expectOne('/api/pets/p1/adoption-requests')
      .flush({ detail: 'Conflito qualquer.' }, { status: 409, statusText: 'Conflict' });
    harness.detectChanges();

    expect(el.querySelector('[data-testid="pending-badge"]')).toBeNull();
    expect(TestBed.inject(ToastService).toasts()[0].message).toBe('Conflito qualquer.');
  });

  it('retries the load from the error banner', async () => {
    await harness.navigateByUrl('/pets/p1');
    http.expectOne('/api/pets/p1').flush({}, { status: 500, statusText: 'Server Error' });
    harness.detectChanges();
    const el = harness.routeNativeElement as HTMLElement;
    button(el, 'Tentar novamente')!.click();
    http.expectOne('/api/pets/p1').flush(PET);
    harness.detectChanges();
    expect(el.textContent).toContain('Rex');
  });

  it('sets the document title and announces it after the pet loads', async () => {
    vi.useFakeTimers({ toFake: ['setTimeout', 'clearTimeout'] });
    try {
      await open();
      vi.advanceTimersByTime(60);
      expect(document.title).toBe('Rex — AdotPet');
      expect(TestBed.inject(RouteAnnouncer).message()).toBe('Rex — AdotPet');
    } finally {
      vi.useRealTimers();
    }
  });

  it('announces the failure states instead of staying silent', async () => {
    vi.useFakeTimers({ toFake: ['setTimeout', 'clearTimeout'] });
    try {
      const announcer = TestBed.inject(RouteAnnouncer);
      await harness.navigateByUrl('/pets/p1');
      http.expectOne('/api/pets/p1').flush({}, { status: 404, statusText: 'Not Found' });
      vi.advanceTimersByTime(60);
      expect(announcer.message()).toBe('Pet não encontrado — AdotPet');

      await harness.navigateByUrl('/pets/p2');
      http.expectOne('/api/pets/p2').flush({}, { status: 500, statusText: 'Server Error' });
      vi.advanceTimersByTime(60);
      expect(announcer.message()).toBe('Erro ao carregar o pet — AdotPet');
    } finally {
      vi.useRealTimers();
    }
  });

  it('shows a not-found message on 404', async () => {
    await harness.navigateByUrl('/pets/p1');
    http.expectOne('/api/pets/p1').flush({}, { status: 404, statusText: 'Not Found' });
    harness.detectChanges();
    expect((harness.routeNativeElement as HTMLElement).textContent).toContain(
      'Pet não encontrado.',
    );
  });
});
