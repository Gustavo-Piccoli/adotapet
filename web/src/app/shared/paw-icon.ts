import { ChangeDetectionStrategy, Component } from '@angular/core';

/** Decorative paw print; size and colour come from the host's classes (currentColor). */
@Component({
  selector: 'app-paw-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'inline-block', 'aria-hidden': 'true' },
  template: `
    <svg viewBox="0 0 24 24" fill="currentColor" class="size-full">
      <ellipse cx="6" cy="10" rx="2" ry="2.6" />
      <ellipse cx="10.5" cy="6" rx="2" ry="2.6" />
      <ellipse cx="15.5" cy="6" rx="2" ry="2.6" />
      <ellipse cx="19.5" cy="10" rx="2" ry="2.6" />
      <path
        d="M12.5 11c-3 0-6 3.6-6 6.1 0 1.7 1.3 2.4 2.9 2.4 1 0 1.9-.4 3.1-.4s2.1.4 3.1.4c1.6 0 2.9-.7 2.9-2.4 0-2.5-3-6.1-6-6.1z"
      />
    </svg>
  `,
})
export class PawIcon {}
