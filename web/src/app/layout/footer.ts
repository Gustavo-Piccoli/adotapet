import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <footer class="mt-16 bg-pine-900 text-pine-100">
      <div class="container-page flex flex-col gap-1 py-8 text-sm">
        <p class="font-display text-lg font-semibold text-white">AdotaPet</p>
        <p>Adoção responsável começa com um encontro. Projeto de portfólio feito com carinho.</p>
      </div>
    </footer>
  `,
})
export class Footer {}
