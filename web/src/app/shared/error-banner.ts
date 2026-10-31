import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-error-banner',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      class="flex flex-wrap items-center justify-between gap-3 rounded-card bg-red-50 px-5 py-4 text-red-800"
      role="alert"
    >
      <span>{{ message() }}</span>
      <button type="button" class="btn btn-secondary" (click)="retry.emit()">
        Tentar novamente
      </button>
    </div>
  `,
})
export class ErrorBanner {
  readonly message = input.required<string>();
  readonly retry = output<void>();
}
