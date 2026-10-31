import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { roleHome, safeReturnUrl } from '../../core/auth/redirect';
import { ToastService } from '../../core/toast/toast.service';
import { FieldA11y, FieldError } from '../../shared/field-error';
import { UFS } from '../../shared/labels';
import {
  applyMask,
  formatPhone,
  onlyDigits,
  passwordValidator,
  phoneValidator,
} from '../../shared/validators';

@Component({
  selector: 'app-register-adopter-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, FieldError, FieldA11y],
  template: `
    <section class="container-page py-10 sm:py-14">
      <div class="card mx-auto max-w-xl p-6 sm:p-8">
        <h1 class="text-3xl font-bold text-pine-900">Criar conta de adotante</h1>
        <p class="mt-1 text-ink-soft">Cadastre-se para enviar pedidos de adoção.</p>

        <form class="mt-6 grid gap-4 sm:grid-cols-2" [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <div class="sm:col-span-2">
            <label class="field-label" for="a-name">Nome completo</label>
            <input id="a-name" type="text" class="field-input" formControlName="name" autocomplete="name" />
            <app-field-error [control]="form.controls.name" />
          </div>
          <div>
            <label class="field-label" for="a-email">E-mail</label>
            <input id="a-email" type="email" class="field-input" formControlName="email" autocomplete="email" />
            <app-field-error [control]="form.controls.email" />
          </div>
          <div>
            <label class="field-label" for="a-phone">Telefone</label>
            <input
              id="a-phone"
              type="tel"
              class="field-input"
              formControlName="phone"
              placeholder="(51) 99999-9999"
              autocomplete="tel"
              (input)="mask($event)"
            />
            <app-field-error [control]="form.controls.phone" />
          </div>
          <div>
            <label class="field-label" for="a-city">Cidade</label>
            <input id="a-city" type="text" class="field-input" formControlName="city" autocomplete="address-level2" />
            <app-field-error [control]="form.controls.city" />
          </div>
          <div>
            <label class="field-label" for="a-state">UF</label>
            <select id="a-state" class="field-input" formControlName="state">
              <option value="">Selecione</option>
              @for (uf of ufs; track uf) {
                <option [value]="uf">{{ uf }}</option>
              }
            </select>
            <app-field-error [control]="form.controls.state" />
          </div>
          <div class="sm:col-span-2">
            <label class="field-label" for="a-password">Senha</label>
            <input
              id="a-password"
              type="password"
              class="field-input"
              formControlName="password"
              autocomplete="new-password"
              aria-describedby="a-password-hint"
            />
            <p id="a-password-hint" class="mt-1 text-xs text-ink-soft">
              Mínimo de 8 caracteres, com ao menos um número e uma letra minúscula.
            </p>
            <app-field-error [control]="form.controls.password" />
          </div>
          <div class="sm:col-span-2">
            <button type="submit" class="btn btn-primary w-full" [disabled]="submitting()">
              {{ submitting() ? 'Criando conta...' : 'Criar conta' }}
            </button>
          </div>
        </form>

        <div class="mt-6 grid gap-1 border-t border-cream-300 pt-4 text-sm text-ink-soft">
          <p>
            Já tem conta?
            <a routerLink="/entrar" class="font-semibold text-clay-700 underline">Entrar</a>
          </p>
          <p>
            É uma ONG?
            <a routerLink="/cadastro/ong" class="font-semibold text-clay-700 underline"
              >Cadastre sua ONG</a
            >
          </p>
        </div>
      </div>
    </section>
  `,
})
export class RegisterAdopterPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly ufs = UFS;
  protected readonly submitting = signal(false);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    phone: ['', [Validators.required, phoneValidator]],
    city: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(80)]],
    state: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(8), passwordValidator]],
  });

  protected mask(event: Event): void {
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
      .registerAdopter({
        ...v,
        name: v.name.trim(),
        city: v.city.trim(),
        phone: onlyDigits(v.phone),
      })
      .subscribe({
        next: (result) => {
          const target =
            safeReturnUrl(this.route.snapshot.queryParamMap.get('returnUrl')) ??
            roleHome(result.role);
          void this.router.navigateByUrl(target);
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          this.toast.error(this.toast.problemMessage(err));
        },
      });
  }
}
