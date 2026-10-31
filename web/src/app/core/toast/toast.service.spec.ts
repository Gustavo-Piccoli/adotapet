import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { GENERIC_ERROR_MESSAGE, TOAST_DURATION_MS, ToastService } from './toast.service';

describe('ToastService', () => {
  let toast: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    toast = TestBed.inject(ToastService);
  });
  afterEach(() => vi.useRealTimers());

  it('adds toasts and auto-dismisses them after 5s', () => {
    toast.success('ok');
    toast.error('ruim');
    expect(toast.toasts().map((t) => t.kind)).toEqual(['success', 'error']);
    vi.advanceTimersByTime(TOAST_DURATION_MS - 1);
    expect(toast.toasts().length).toBe(2);
    vi.advanceTimersByTime(1);
    expect(toast.toasts().length).toBe(0);
  });

  it('problemMessage picks detail', () => {
    const err = new HttpErrorResponse({ status: 409, error: { detail: 'Pedido em análise.' } });
    expect(toast.problemMessage(err)).toBe('Pedido em análise.');
  });

  it('problemMessage falls back to the first validation error', () => {
    const err = new HttpErrorResponse({
      status: 400,
      error: { errors: { Email: ['E-mail inválido.', 'outro'] } },
    });
    expect(toast.problemMessage(err)).toBe('E-mail inválido.');
  });

  it('problemMessage falls back to the generic message', () => {
    expect(toast.problemMessage(new HttpErrorResponse({ status: 500 }))).toBe(GENERIC_ERROR_MESSAGE);
    expect(GENERIC_ERROR_MESSAGE).toBe('Algo deu errado. Tente novamente.');
  });
});
