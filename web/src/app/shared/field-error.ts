import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  ElementRef,
  OnDestroy,
  OnInit,
  inject,
  input,
} from '@angular/core';
import { AbstractControl, NgControl, Validators } from '@angular/forms';

export function errorMessage(control: AbstractControl): string | null {
  const e = control.errors;
  if (!e) return null;
  if (e['required']) return 'Campo obrigatório.';
  if (e['email']) return 'Informe um e-mail válido.';
  if (e['minlength']) return `Informe no mínimo ${e['minlength'].requiredLength} caracteres.`;
  if (e['maxlength']) return `Informe no máximo ${e['maxlength'].requiredLength} caracteres.`;
  if (e['cnpj']) return 'CNPJ inválido.';
  if (e['phone']) return 'Informe um telefone com DDD (10 ou 11 dígitos).';
  if (e['password']) return 'A senha deve ter ao menos um número e uma letra minúscula.';
  return 'Valor inválido.';
}

/** Maps a control to the id of its <app-field-error>, so the input can point to it. */
const errorIds = new WeakMap<AbstractControl, string>();
let nextErrorId = 0;

/** True once the error should be shown: only after the user left the field or submitted. */
function showsError(control: AbstractControl | null): boolean {
  return !!control && control.touched && control.invalid;
}

@Component({
  selector: 'app-field-error',
  // Default strategy on purpose: the control is mutated in place (touched/errors), so an OnPush
  // child would never refresh when its input reference stays the same.
  changeDetection: ChangeDetectionStrategy.Default,
  host: { '[id]': 'id', 'aria-live': 'polite', class: 'block' },
  template: `
    @if (control().touched) {
      @if (message(); as m) {
        <p class="field-error">{{ m }}</p>
      }
    }
  `,
})
export class FieldError implements OnInit, OnDestroy {
  readonly control = input.required<AbstractControl>();
  protected readonly id = `field-error-${nextErrorId++}`;

  ngOnInit(): void {
    errorIds.set(this.control(), this.id);
  }

  ngOnDestroy(): void {
    if (errorIds.get(this.control()) === this.id) errorIds.delete(this.control());
  }

  protected message(): string | null {
    return errorMessage(this.control());
  }
}

/**
 * Accessibility attributes for text-like form fields: aria-invalid once the error is visible,
 * aria-required for required controls, and aria-describedby pointing at the field error
 * (kept next to any hint already declared on the element).
 */
@Directive({
  // Matches the form fields themselves so templates need no extra attribute.
  // eslint-disable-next-line @angular-eslint/directive-selector
  selector: 'input:not([type=checkbox])[formControlName], select[formControlName], textarea[formControlName]',
  host: {
    '[attr.aria-invalid]': 'invalid() ? "true" : null',
    '[attr.aria-required]': 'required() ? "true" : null',
    '[attr.aria-describedby]': 'describedBy()',
  },
})
export class FieldA11y {
  private readonly ngControl = inject(NgControl);
  // Static aria-describedby from the template (a hint id), captured before the binding overrides it.
  private readonly hint = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement.getAttribute(
    'aria-describedby',
  );

  protected invalid(): boolean {
    return showsError(this.ngControl.control);
  }

  protected required(): boolean {
    return this.ngControl.control?.hasValidator(Validators.required) ?? false;
  }

  protected describedBy(): string | null {
    const errorId = this.ngControl.control ? errorIds.get(this.ngControl.control) : undefined;
    return [this.hint, errorId].filter(Boolean).join(' ') || null;
  }
}
