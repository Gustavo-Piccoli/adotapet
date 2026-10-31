import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <footer class="focus-light mt-16 bg-pine-900 text-pine-100">
      <div class="container-page flex flex-col gap-1 py-8 text-sm">
        <p class="font-display text-lg font-semibold text-white">AdotPet</p>
        <p>Adoção responsável começa com um encontro. Projeto de portfólio feito com carinho.</p>
        <p>
          É uma ONG?
          <a routerLink="/cadastro/ong" class="font-semibold text-white underline">Cadastre-se</a>
        </p>
      </div>
    </footer>
  `,
})
export class Footer {}
