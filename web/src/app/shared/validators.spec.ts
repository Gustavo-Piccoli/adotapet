import { FormControl } from '@angular/forms';
import {
  cnpjValidator,
  formatCnpj,
  formatPhone,
  isValidCnpj,
  passwordValidator,
  phoneValidator,
} from './validators';

describe('validators', () => {
  it('accepts valid CNPJs, masked or not', () => {
    expect(isValidCnpj('11.222.333/0001-81')).toBe(true);
    expect(isValidCnpj('11222333000181')).toBe(true);
  });

  it('rejects wrong check digits, repeated digits and wrong length', () => {
    expect(isValidCnpj('11.222.333/0001-82')).toBe(false);
    expect(isValidCnpj('11111111111111')).toBe(false);
    expect(isValidCnpj('1122233300018')).toBe(false);
    expect(isValidCnpj('')).toBe(false);
  });

  it('cnpjValidator ignores empty values (required handles them)', () => {
    expect(cnpjValidator(new FormControl(''))).toBeNull();
    expect(cnpjValidator(new FormControl('11.222.333/0001-82'))).toEqual({ cnpj: true });
  });

  it('phoneValidator requires 10 or 11 digits', () => {
    expect(phoneValidator(new FormControl('(51) 99999-8888'))).toBeNull();
    expect(phoneValidator(new FormControl('(51) 3333-4444'))).toBeNull();
    expect(phoneValidator(new FormControl('(51) 9999'))).toEqual({ phone: true });
  });

  it('passwordValidator needs a digit and a lowercase letter', () => {
    expect(passwordValidator(new FormControl('abcdefg1'))).toBeNull();
    expect(passwordValidator(new FormControl('ABCDEFG1'))).toEqual({ password: true });
    expect(passwordValidator(new FormControl('abcdefgh'))).toEqual({ password: true });
  });

  it('formats masks progressively', () => {
    expect(formatCnpj('11222333000181')).toBe('11.222.333/0001-81');
    expect(formatCnpj('112')).toBe('11.2');
    expect(formatPhone('51999998888')).toBe('(51) 99999-8888');
    expect(formatPhone('5133334444')).toBe('(51) 3333-4444');
    expect(formatPhone('5')).toBe('(5');
  });
});
