import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { PawIcon } from '../shared/paw-icon';

@Component({
  selector: 'app-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, PawIcon],
  template: `
    <header class="sticky top-0 z-40 border-b border-cream-300/70 bg-cream-100/90 backdrop-blur">
      <div class="container-page flex flex-wrap items-center justify-between gap-x-6 gap-y-2 py-3">
        <a routerLink="/" class="flex items-center gap-2 font-display text-2xl font-bold text-pine-900">
          <span class="grid size-9 place-items-center rounded-full bg-clay-600 text-white">
            <app-paw-icon class="size-5" />
          </span>
          AdotPet
        </a>
        <nav aria-label="Principal" class="flex flex-wrap items-center gap-1">
          @if (auth.isShelter()) {
            <a routerLink="/ong/pets" routerLinkActive="bg-cream-200" class="btn btn-ghost">Meus pets</a>
            <a routerLink="/ong/pedidos" routerLinkActive="bg-cream-200" class="btn btn-ghost">Pedidos</a>
            <button type="button" class="btn btn-secondary" (click)="logout()">Sair</button>
          } @else if (auth.isAdopter()) {
            <a routerLink="/meus-pedidos" routerLinkActive="bg-cream-200" class="btn btn-ghost">Meus pedidos</a>
            <button type="button" class="btn btn-secondary" (click)="logout()">Sair</button>
          } @else {
            <a routerLink="/entrar" routerLinkActive="bg-cream-200" class="btn btn-ghost">Entrar</a>
            <a routerLink="/cadastro/adotante" class="btn btn-primary">Cadastre-se</a>
          }
        </nav>
      </div>
    </header>
  `,
})
export class Header {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/');
  }
}
