import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FieldA11y, FieldError } from './field-error';

@Component({
  imports: [ReactiveFormsModule, FieldError, FieldA11y],
  template: `
    <form [formGroup]="form">
      <input id="n" formControlName="name" aria-describedby="hint" />
      <app-field-error [control]="form.controls.name" />
    </form>
  `,
})
class Host {
  form = new FormBuilder().nonNullable.group({ name: ['', Validators.required] });
}

describe('FieldError / FieldA11y', () => {
  const setup = () => {
    const fixture = TestBed.createComponent(Host);
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    return { fixture, el, input: el.querySelector('input')!, error: el.querySelector('app-field-error')! };
  };

  it('stays silent until the field is touched, even while dirty', () => {
    const { fixture, el, input } = setup();
    fixture.componentInstance.form.controls.name.markAsDirty();
    fixture.detectChanges();
    expect(el.querySelector('.field-error')).toBeNull();
    expect(input.getAttribute('aria-invalid')).toBeNull();

    fixture.componentInstance.form.controls.name.markAsTouched();
    fixture.detectChanges();
    expect(el.querySelector('.field-error')?.textContent).toContain('Campo obrigatório.');
    expect(input.getAttribute('aria-invalid')).toBe('true');
  });

  it('uses a polite live region instead of role=alert and links it to the input', () => {
    const { input, error, el } = setup();
    expect(error.getAttribute('aria-live')).toBe('polite');
    expect(el.querySelector('[role="alert"]')).toBeNull();
    expect(input.getAttribute('aria-describedby')).toBe(`hint ${error.id}`);
    expect(input.getAttribute('aria-required')).toBe('true');
  });
});
