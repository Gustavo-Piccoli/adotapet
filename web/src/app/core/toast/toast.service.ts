import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { ProblemDetails } from '../http/problem-details';

export type ToastKind = 'success' | 'error';

export interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

export const TOAST_DURATION_MS = 5000;
export const GENERIC_ERROR_MESSAGE = 'Algo deu errado. Tente novamente.';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();

  success(message: string): void {
    this.push('success', message);
  }

  error(message: string): void {
    this.push('error', message);
  }

  dismiss(id: number): void {
    this._toasts.update((list) => list.filter((t) => t.id !== id));
  }

  problemMessage(err: HttpErrorResponse): string {
    const problem = err.error as ProblemDetails | null;
    if (problem && typeof problem === 'object') {
      if (typeof problem.detail === 'string' && problem.detail) return problem.detail;
      const first = Object.values(problem.errors ?? {}).flat()[0];
      if (first) return first;
    }
    return GENERIC_ERROR_MESSAGE;
  }

  private push(kind: ToastKind, message: string): void {
    const id = this.nextId++;
    this._toasts.update((list) => [...list, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), TOAST_DURATION_MS);
  }
}
