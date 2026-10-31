import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { roleHome, safeReturnUrl } from '../../core/auth/redirect';
import { ToastService } from '../../core/toast/toast.service';
import { FieldA11y, FieldError } from '../../shared/field-error';

@Component({
  selector: 'app-login-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, FieldError, FieldA11y],
  template: `
    <section class="container-page py-10 sm:py-14">
      <div class="card mx-auto max-w-md p-6 sm:p-8">
        <h1 class="text-3xl font-bold text-pine-900">Entrar</h1>
        <p class="mt-1 text-ink-soft">Acesse sua conta para acompanhar seus pedidos.</p>

        <form class="mt-6 grid gap-4" [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <div>
            <label class="field-label" for="l-email">E-mail</label>
            <input
              id="l-email"
              type="email"
              class="field-input"
              formControlName="email"
              autocomplete="email"
            />
            <app-field-error [control]="form.controls.email" />
          </div>
          <div>
            <label class="field-label" for="l-password">Senha</label>
            <input
              id="l-password"
              type="password"
              class="field-input"
              formControlName="password"
              autocomplete="current-password"
            />
            <app-field-error [control]="form.controls.password" />
          </div>
          <button type="submit" class="btn btn-primary" [disabled]="submitting()">
            {{ submitting() ? 'Entrando...' : 'Entrar' }}
          </button>
        </form>

        <div class="mt-6 grid gap-1 border-t border-cream-300 pt-4 text-sm text-ink-soft">
          <p>
            Quer adotar?
            <a routerLink="/cadastro/adotante" queryParamsHandling="preserve" class="font-semibold text-clay-700 underline"
              >Crie sua conta de adotante</a
            >
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
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly submitting = signal(false);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    password: ['', [Validators.required]],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.auth.login(this.form.getRawValue()).subscribe({
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
