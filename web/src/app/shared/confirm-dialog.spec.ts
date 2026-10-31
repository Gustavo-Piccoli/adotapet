import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ConfirmDialog } from './confirm-dialog';

@Component({
  imports: [ConfirmDialog],
  template: `
    <button id="opener" type="button" (click)="open.set(true)">Abrir</button>
    @if (open()) {
      <app-confirm-dialog message="Certeza?" (confirmed)="open.set(false)" (cancelled)="open.set(false)" />
    }
  `,
})
class Host {
  readonly open = signal(false);
}

describe('ConfirmDialog', () => {
  let el: HTMLElement;
  let fixture: ReturnType<typeof TestBed.createComponent<Host>>;

  const buttons = () => Array.from(el.querySelectorAll<HTMLButtonElement>('app-confirm-dialog button'));
  const key = (name: string, shiftKey = false) => {
    const ev = new KeyboardEvent('keydown', { key: name, shiftKey, bubbles: true, cancelable: true });
    (document.activeElement as HTMLElement).dispatchEvent(ev);
    return ev;
  };

  beforeEach(async () => {
    fixture = TestBed.createComponent(Host);
    document.body.appendChild(fixture.nativeElement);
    el = fixture.nativeElement;
    await fixture.whenStable();
    const opener = el.querySelector<HTMLButtonElement>('#opener')!;
    opener.focus();
    opener.click();
    await fixture.whenStable();
  });

  afterEach(() => fixture.nativeElement.remove());

  it('focuses "Continuar" initially', () => {
    expect(document.activeElement?.textContent?.trim()).toBe('Continuar');
  });

  it('traps Tab and Shift+Tab inside the dialog', () => {
    const [cancel, confirm] = buttons();
    expect(key('Tab').defaultPrevented).toBe(true);
    expect(document.activeElement).toBe(cancel);
    cancel.focus();
    expect(key('Tab', true).defaultPrevented).toBe(true);
    expect(document.activeElement).toBe(confirm);
  });

  it('Esc cancels and returns focus to the opener', async () => {
    key('Escape');
    await fixture.whenStable();
    expect(el.querySelector('app-confirm-dialog')).toBeNull();
    expect(document.activeElement).toBe(el.querySelector('#opener'));
  });

  it('confirming returns focus to the opener', async () => {
    buttons()[1].click();
    await fixture.whenStable();
    expect(document.activeElement).toBe(el.querySelector('#opener'));
  });
});
