import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const onlyDigits = (value: string | null | undefined): string =>
  (value ?? '').replace(/\D/g, '');

/** Pure CNPJ check-digit validation (accepts masked or digits-only input). */
export function isValidCnpj(value: string | null | undefined): boolean {
  const digits = onlyDigits(value);
  if (digits.length !== 14 || /^(\d)\1{13}$/.test(digits)) return false;
  const checkDigit = (length: number): number => {
    let weight = length - 7;
    let sum = 0;
    for (let i = 0; i < length; i++) {
      sum += Number(digits[i]) * weight--;
      if (weight < 2) weight = 9;
    }
    const rest = sum % 11;
    return rest < 2 ? 0 : 11 - rest;
  };
  return checkDigit(12) === Number(digits[12]) && checkDigit(13) === Number(digits[13]);
}

export const cnpjValidator: ValidatorFn = (c: AbstractControl): ValidationErrors | null =>
  !c.value || isValidCnpj(c.value) ? null : { cnpj: true };

export const phoneValidator: ValidatorFn = (c: AbstractControl): ValidationErrors | null => {
  if (!c.value) return null;
  const length = onlyDigits(c.value).length;
  return length === 10 || length === 11 ? null : { phone: true };
};

export const passwordValidator: ValidatorFn = (c: AbstractControl): ValidationErrors | null => {
  const v = (c.value as string) ?? '';
  if (!v) return null;
  return /\d/.test(v) && /\p{Ll}/u.test(v) ? null : { password: true };
};

export function formatCnpj(value: string): string {
  const d = onlyDigits(value).slice(0, 14);
  let out = d.slice(0, 2);
  if (d.length > 2) out += '.' + d.slice(2, 5);
  if (d.length > 5) out += '.' + d.slice(5, 8);
  if (d.length > 8) out += '/' + d.slice(8, 12);
  if (d.length > 12) out += '-' + d.slice(12, 14);
  return out;
}

export function formatPhone(value: string): string {
  const d = onlyDigits(value).slice(0, 11);
  if (d.length === 0) return '';
  if (d.length <= 2) return `(${d}`;
  const split = d.length > 10 ? 7 : 6;
  const head = `(${d.slice(0, 2)}) ${d.slice(2, split)}`;
  return d.length > split ? `${head}-${d.slice(split)}` : head;
}

/** Re-formats the control value as the user types (display-only mask). */
export function applyMask(
  event: Event,
  control: AbstractControl,
  format: (value: string) => string,
): void {
  const raw = (event.target as HTMLInputElement).value;
  control.setValue(format(raw));
}
