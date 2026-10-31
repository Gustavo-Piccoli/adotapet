import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  inject,
  input,
  output,
  viewChild,
} from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(keydown.escape)': 'cancelled.emit()',
    '(keydown.tab)': 'trapTab($event)',
    '(keydown.shift.tab)': 'trapTab($event)',
  },
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
          <button
            #confirmButton
            type="button"
            class="btn"
            [class.btn-primary]="!danger()"
            [class.btn-danger]="danger()"
            (click)="confirmed.emit()"
          >
            {{ confirmLabel() }}
          </button>
        </div>
      </div>
    </div>
  `,
})
export class ConfirmDialog {
  readonly message = input.required<string>();
  readonly confirmLabel = input('Continuar');
  readonly danger = input(false);
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  private readonly confirmButton =
    viewChild.required<ElementRef<HTMLButtonElement>>('confirmButton');

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly opener = document.activeElement as HTMLElement | null;

  constructor() {
    afterNextRender(() => this.confirmButton().nativeElement.focus());
    inject(DestroyRef).onDestroy(() => this.opener?.focus?.());
  }

  /** Keeps Tab / Shift+Tab cycling inside the dialog buttons. */
  protected trapTab(event: Event): void {
    const e = event as KeyboardEvent;
    const buttons = Array.from(this.host.nativeElement.querySelectorAll('button'));
    if (buttons.length === 0) return;
    const first = buttons[0];
    const last = buttons[buttons.length - 1];
    const active = document.activeElement;
    if (e.shiftKey && (active === first || !buttons.includes(active as HTMLButtonElement))) {
      e.preventDefault();
      last.focus();
    } else if (!e.shiftKey && (active === last || !buttons.includes(active as HTMLButtonElement))) {
      e.preventDefault();
      first.focus();
    }
  }
}
