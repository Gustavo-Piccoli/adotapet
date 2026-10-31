import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { PawIcon } from './paw-icon';

@Component({
  selector: 'app-empty-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PawIcon],
  template: `
    <div class="empty-state" role="status">
      <app-paw-icon class="mx-auto mb-4 block size-14 text-clay-500" />
      <p class="font-display text-lg font-semibold text-ink">{{ message() }}</p>
      <ng-content />
    </div>
  `,
})
export class EmptyState {
  readonly message = input.required<string>();
}
