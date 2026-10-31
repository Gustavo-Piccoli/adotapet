import { TestBed } from '@angular/core/testing';
import { Paginator } from './paginator';

function render(page: number, totalCount: number) {
  const fixture = TestBed.createComponent(Paginator);
  fixture.componentRef.setInput('page', page);
  fixture.componentRef.setInput('pageSize', 12);
  fixture.componentRef.setInput('totalCount', totalCount);
  fixture.detectChanges();
  const buttons = Array.from(
    (fixture.nativeElement as HTMLElement).querySelectorAll('button'),
  );
  const byText = (t: string) => buttons.find((b) => b.textContent?.trim() === t);
  return { el: fixture.nativeElement as HTMLElement, byText };
}

describe('Paginator', () => {
  it('disables Anterior on the first page', () => {
    const { byText } = render(1, 30);
    expect(byText('Anterior')!.disabled).toBe(true);
    expect(byText('Próxima')!.disabled).toBe(false);
  });

  it('disables Próxima on the last page', () => {
    const { byText } = render(3, 30);
    expect(byText('Próxima')!.disabled).toBe(true);
    expect(byText('Anterior')!.disabled).toBe(false);
  });

  it('renders nothing for a single page', () => {
    expect(render(1, 5).el.querySelector('nav')).toBeNull();
  });
});
