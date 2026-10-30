import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { roleHome } from '../../core/auth/redirect';
import { ToastService } from '../../core/toast/toast.service';
import { FieldError } from '../../shared/field-error';
import { UFS } from '../../shared/labels';
import {
  applyMask,
  cnpjValidator,
  formatCnpj,
  formatPhone,
  onlyDigits,
  passwordValidator,
  phoneValidator,
} from '../../shared/validators';

@Component({
  selector: 'app-register-shelter-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, FieldError],
  template: `
    <section class="container-page py-10 sm:py-14">
      <div class="card mx-auto max-w-2xl p-6 sm:p-8">
        <h1 class="text-3xl font-bold text-pine-900">Cadastrar ONG</h1>
        <p class="mt-1 text-ink-soft">Publique seus pets e receba pedidos de adoção.</p>

        <form class="mt-6 grid gap-4 sm:grid-cols-2" [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <div class="sm:col-span-2">
            <label class="field-label" for="s-name">Nome da ONG</label>
            <input id="s-name" type="text" class="field-input" formControlName="name" autocomplete="organization" />
            <app-field-error [control]="form.controls.name" />
          </div>
          <div>
            <label class="field-label" for="s-cnpj">CNPJ</label>
            <input
              id="s-cnpj"
              type="text"
              inputmode="numeric"
              class="field-input"
              formControlName="cnpj"
              placeholder="00.000.000/0000-00"
              (input)="maskCnpj($event)"
            />
            <app-field-error [control]="form.controls.cnpj" />
          </div>
          <div>
            <label class="field-label" for="s-phone">Telefone</label>
            <input
              id="s-phone"
              type="tel"
              class="field-input"
              formControlName="phone"
              placeholder="(51) 99999-9999"
              autocomplete="tel"
              (input)="maskPhone($event)"
            />
            <app-field-error [control]="form.controls.phone" />
          </div>
          <div>
            <label class="field-label" for="s-city">Cidade</label>
            <input id="s-city" type="text" class="field-input" formControlName="city" autocomplete="address-level2" />
            <app-field-error [control]="form.controls.city" />
          </div>
          <div>
            <label class="field-label" for="s-state">UF</label>
            <select id="s-state" class="field-input" formControlName="state">
              <option value="">Selecione</option>
              @for (uf of ufs; track uf) {
                <option [value]="uf">{{ uf }}</option>
              }
            </select>
            <app-field-error [control]="form.controls.state" />
          </div>
          <div class="sm:col-span-2">
            <label class="field-label" for="s-description">Sobre a ONG (opcional)</label>
            <textarea id="s-description" rows="4" class="field-input" formControlName="description"></textarea>
            <app-field-error [control]="form.controls.description" />
          </div>
          <div>
            <label class="field-label" for="s-email">E-mail</label>
            <input id="s-email" type="email" class="field-input" formControlName="email" autocomplete="email" />
            <app-field-error [control]="form.controls.email" />
          </div>
          <div>
            <label class="field-label" for="s-password">Senha</label>
            <input
              id="s-password"
              type="password"
              class="field-input"
              formControlName="password"
              autocomplete="new-password"
              aria-describedby="s-password-hint"
            />
            <app-field-error [control]="form.controls.password" />
          </div>
          <p id="s-password-hint" class="-mt-2 text-xs text-ink-soft sm:col-span-2">
            Senha com no mínimo 8 caracteres, ao menos um número e uma letra minúscula.
          </p>
          <div class="sm:col-span-2">
            <button type="submit" class="btn btn-primary w-full" [disabled]="submitting()">
              {{ submitting() ? 'Cadastrando...' : 'Cadastrar ONG' }}
            </button>
          </div>
        </form>

        <div class="mt-6 grid gap-1 border-t border-cream-300 pt-4 text-sm text-ink-soft">
          <p>
            Já tem conta?
            <a routerLink="/entrar" class="font-semibold text-clay-700 underline">Entrar</a>
          </p>
          <p>
            Quer adotar?
            <a routerLink="/cadastro/adotante" class="font-semibold text-clay-700 underline"
              >Crie sua conta de adotante</a
            >
          </p>
        </div>
      </div>
    </section>
  `,
})
export class RegisterShelterPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly ufs = UFS;
  protected readonly submitting = signal(false);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    cnpj: ['', [Validators.required, cnpjValidator]],
    phone: ['', [Validators.required, phoneValidator]],
    city: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    state: ['', [Validators.required]],
    description: ['', [Validators.maxLength(1000)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    password: ['', [Validators.required, Validators.minLength(8), passwordValidator]],
  });

  protected maskCnpj(event: Event): void {
    applyMask(event, this.form.controls.cnpj, formatCnpj);
  }

  protected maskPhone(event: Event): void {
    applyMask(event, this.form.controls.phone, formatPhone);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    const v = this.form.getRawValue();
    this.auth
      .registerShelter({
        ...v,
        name: v.name.trim(),
        city: v.city.trim(),
        description: v.description.trim(),
        cnpj: onlyDigits(v.cnpj),
        phone: onlyDigits(v.phone),
      })
      .subscribe({
        next: (result) => void this.router.navigateByUrl(roleHome(result.role)),
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          this.toast.error(this.toast.problemMessage(err));
        },
      });
  }
}
