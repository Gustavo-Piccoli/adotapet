import { Injectable, signal } from '@angular/core';

/** Text of the sr-only live region in the shell. */
@Injectable({ providedIn: 'root' })
export class RouteAnnouncer {
  readonly message = signal('');
  private timer: ReturnType<typeof setTimeout> | undefined;

  /** Clears the region first so identical consecutive texts are announced again. */
  announce(text: string): void {
    clearTimeout(this.timer);
    this.message.set('');
    this.timer = setTimeout(() => this.message.set(text), 50);
  }
}
