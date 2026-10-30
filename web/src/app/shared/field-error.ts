import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

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

@Component({
  selector: 'app-field-error',
  // Default strategy on purpose: the control is mutated in place (touched/errors), so an OnPush
  // child would never refresh when its input reference stays the same.
  changeDetection: ChangeDetectionStrategy.Default,
  template: `
    @if (control().touched || control().dirty) {
      @if (message(); as m) {
        <p class="field-error" role="alert">{{ m }}</p>
      }
    }
  `,
})
export class FieldError {
  readonly control = input.required<AbstractControl>();
  protected message(): string | null {
    return errorMessage(this.control());
  }
}
