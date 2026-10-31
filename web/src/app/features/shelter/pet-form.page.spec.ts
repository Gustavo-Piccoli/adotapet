import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { ToastService } from '../../core/toast/toast.service';
import { PetFormPage, photoError } from './pet-form.page';

@Component({ template: 'stub' })
class Stub {}

describe('PetFormPage', () => {
  let http: HttpTestingController;
  let harness: RouterTestingHarness;
  let el: HTMLElement;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'ong/pets/novo', component: PetFormPage },
          { path: 'ong/pets/:id/editar', component: PetFormPage },
          { path: 'ong/pets', component: Stub },
        ]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    harness = await RouterTestingHarness.create();
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:preview');
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => undefined);
  });

  afterEach(() => {
    http.verify();
    vi.restoreAllMocks();
  });

  const setValue = (selector: string, value: string, event = 'input') => {
    const control = el.querySelector<HTMLInputElement>(selector)!;
    control.value = value;
    control.dispatchEvent(new Event(event));
  };

  const fillValid = () => {
    setValue('#p-name', 'Rex');
    setValue('#p-species', 'Dog', 'change');
    setValue('#p-sex', 'Male', 'change');
    setValue('#p-size', 'Small', 'change');
    setValue('#p-age', 'Adult', 'change');
    setValue('#p-description', 'Muito dócil');
  };

  const pickFile = (file: File) => {
    const input = el.querySelector<HTMLInputElement>('#p-photo')!;
    Object.defineProperty(input, 'files', { value: [file], configurable: true });
    input.dispatchEvent(new Event('change'));
    harness.detectChanges();
  };

  const submit = () => el.querySelector('form')!.dispatchEvent(new Event('submit'));

  const open = async (url: string) => {
    await harness.navigateByUrl(url);
    el = harness.routeNativeElement as HTMLElement;
  };

  it('rejects a 6 MB file with a message and does not call the API', async () => {
    await open('/ong/pets/novo');
    const big = new File([new Uint8Array(6 * 1024 * 1024)], 'big.jpg', { type: 'image/jpeg' });
    pickFile(big);
    expect(el.querySelector('[data-testid="photo-error"]')?.textContent).toContain('5 MB');

    fillValid();
    submit();
    http.expectOne('/api/shelter/pets').flush({ id: 'p1' }, { status: 201, statusText: 'Created' });
    // No photo was kept, so no upload request is made.
    http.expectNone('/api/shelter/pets/p1/photo');
  });

  it('rejects a non-image type', () => {
    expect(photoError(new File(['x'], 'a.gif', { type: 'image/gif' }))).toContain('JPEG');
    expect(photoError(new File(['x'], 'a.webp', { type: 'image/webp' }))).toBeNull();
  });

  it('does not call the API when the form is invalid (whitespace-only name)', async () => {
    await open('/ong/pets/novo');
    fillValid();
    setValue('#p-name', '   ');
    submit();
    http.expectNone('/api/shelter/pets');
  });

  it('creates the pet, then uploads the photo, then navigates to the list', async () => {
    await open('/ong/pets/novo');
    fillValid();
    pickFile(new File(['abc'], 'a.png', { type: 'image/png' }));
    submit();
    http.expectOne('/api/shelter/pets').flush({ id: 'p1' }, { status: 201, statusText: 'Created' });
    http.expectOne('/api/shelter/pets/p1/photo').flush(null);
    await harness.fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/ong/pets');
  });

  it('goes to the edit page with an error toast when the photo upload fails after create', async () => {
    await open('/ong/pets/novo');
    fillValid();
    pickFile(new File(['abc'], 'a.png', { type: 'image/png' }));
    submit();
    http.expectOne('/api/shelter/pets').flush({ id: 'p1' }, { status: 201, statusText: 'Created' });
    http
      .expectOne('/api/shelter/pets/p1/photo')
      .flush({ detail: 'Arquivo inválido.' }, { status: 400, statusText: 'Bad Request' });
    await harness.fixture.whenStable();
    http.expectOne('/api/shelter/pets/p1').flush({
      id: 'p1', name: 'Rex', species: 'Dog', sex: 'Male', size: 'Small', ageGroup: 'Adult',
      isNeutered: false, isVaccinated: false, description: 'd', photoUrl: null,
      status: 'Available', createdAt: '2026-01-01T00:00:00Z',
    });
    expect(TestBed.inject(Router).url).toBe('/ong/pets/p1/editar');
    expect(TestBed.inject(ToastService).toasts()[0].message).toContain('Arquivo inválido.');
  });

  const EXISTING = {
    id: 'p9', name: 'Mel', species: 'Cat', sex: 'Female', size: 'Large', ageGroup: 'Senior',
    isNeutered: true, isVaccinated: true, description: 'Calma', photoUrl: '/photos/mel.jpg',
    status: 'Available', createdAt: '2026-01-01T00:00:00Z',
  };

  const value = (selector: string) => el.querySelector<HTMLInputElement>(selector)!.value;
  const checked = (label: string) =>
    Array.from(el.querySelectorAll('label'))
      .find((l) => l.textContent?.includes(label))!
      .querySelector('input')!.checked;

  it('populates the form in edit mode', async () => {
    await open('/ong/pets/p9/editar');
    http.expectOne('/api/shelter/pets/p9').flush(EXISTING);
    harness.detectChanges();
    expect(value('#p-name')).toBe('Mel');
    expect(value('#p-species')).toBe('Cat');
    expect(value('#p-sex')).toBe('Female');
    expect(value('#p-size')).toBe('Large');
    expect(value('#p-age')).toBe('Senior');
    expect(value('#p-description')).toBe('Calma');
    expect(checked('Castrado')).toBe(true);
    expect(checked('Vacinado')).toBe(true);
    expect(el.querySelector('img')?.getAttribute('src')).toBe('/photos/mel.jpg');
  });

  it('edit save PUTs the pet, then the photo when one was picked', async () => {
    await open('/ong/pets/p9/editar');
    http.expectOne('/api/shelter/pets/p9').flush(EXISTING);
    harness.detectChanges();
    pickFile(new File(['abc'], 'a.webp', { type: 'image/webp' }));
    submit();
    const put = http.expectOne((r) => r.method === 'PUT' && r.url === '/api/shelter/pets/p9');
    expect(put.request.body.name).toBe('Mel');
    put.flush(null);
    http.expectOne('/api/shelter/pets/p9/photo').flush(null);
    await harness.fixture.whenStable();
    expect(TestBed.inject(Router).url).toBe('/ong/pets');
  });

  it('ignores a second submit while saving', async () => {
    await open('/ong/pets/novo');
    fillValid();
    submit();
    submit();
    http.expectOne('/api/shelter/pets').flush({ id: 'p1' }, { status: 201, statusText: 'Created' });
  });

  it('revokes the preview URL when the photo changes', async () => {
    await open('/ong/pets/novo');
    pickFile(new File(['abc'], 'a.png', { type: 'image/png' }));
    pickFile(new File(['abd'], 'b.png', { type: 'image/png' }));
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('blob:preview');
  });

  it('shows a friendly not-found state when editing an unknown pet', async () => {
    await open('/ong/pets/zzz/editar');
    http
      .expectOne('/api/shelter/pets/zzz')
      .flush({ detail: 'nf' }, { status: 404, statusText: 'Not Found' });
    harness.detectChanges();
    expect(el.textContent).toContain('Pet não encontrado.');
  });
});
