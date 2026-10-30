import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-container',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      class="fixed bottom-4 right-4 z-50 flex w-80 max-w-[calc(100vw-2rem)] flex-col gap-2"
      aria-live="polite"
    >
      @for (toast of toasts.toasts(); track toast.id) {
        <div
          class="flex items-start justify-between gap-3 rounded-lg px-4 py-3 text-sm text-white shadow-lg"
          [class.bg-green-600]="toast.kind === 'success'"
          [class.bg-red-600]="toast.kind === 'error'"
          [attr.role]="toast.kind === 'error' ? 'alert' : 'status'"
        >
          <span>{{ toast.message }}</span>
          <button
            type="button"
            class="font-bold leading-none"
            aria-label="Fechar"
            (click)="toasts.dismiss(toast.id)"
          >
            ×
          </button>
        </div>
      }
    </div>
  `,
})
export class ToastContainer {
  protected readonly toasts = inject(ToastService);
}
