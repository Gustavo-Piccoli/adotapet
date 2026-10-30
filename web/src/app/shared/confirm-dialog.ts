import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterNextRender,
  input,
  output,
  viewChild,
} from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '(keydown.escape)': 'cancelled.emit()' },
  template: `
    <div class="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4">
      <div
        class="card w-full max-w-md p-6"
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="confirm-dialog-message"
      >
        <p id="confirm-dialog-message" class="text-ink">{{ message() }}</p>
        <div class="mt-5 flex justify-end gap-2">
          <button type="button" class="btn btn-ghost" (click)="cancelled.emit()">Cancelar</button>
          <button #confirmButton type="button" class="btn btn-primary" (click)="confirmed.emit()">
            Continuar
          </button>
        </div>
      </div>
    </div>
  `,
})
export class ConfirmDialog {
  readonly message = input.required<string>();
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  private readonly confirmButton =
    viewChild.required<ElementRef<HTMLButtonElement>>('confirmButton');

  constructor() {
    afterNextRender(() => this.confirmButton().nativeElement.focus());
  }
}
