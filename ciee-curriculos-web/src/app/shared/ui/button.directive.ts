import { Directive, Input, computed, signal } from '@angular/core';
import { cn } from './utils';

export type ButtonVariant = 'default' | 'secondary' | 'outline' | 'ghost' | 'destructive' | 'link';
export type ButtonSize = 'default' | 'sm' | 'lg' | 'icon';

@Directive({
  selector: '[hlmBtn]',
  standalone: true,
  host: {
    '[class]': 'classes()'
  }
})
export class HlmButtonDirective {
  private readonly _variant = signal<ButtonVariant>('default');
  private readonly _size = signal<ButtonSize>('default');
  private readonly _class = signal<string>('');

  @Input()
  set variant(value: ButtonVariant) {
    this._variant.set(value);
  }

  @Input()
  set size(value: ButtonSize) {
    this._size.set(value);
  }

  @Input()
  set class(value: string) {
    this._class.set(value);
  }

  protected readonly classes = computed(() => {
    // Totalmente livre de anéis e contornos externos (ring-0 e outline-none)
    const base = 'inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-semibold transition-colors outline-none focus:outline-none focus-visible:outline-none ring-0 focus:ring-0 focus-visible:ring-0 disabled:pointer-events-none disabled:opacity-50 select-none cursor-pointer';

    const variants: Record<ButtonVariant, string> = {
      default: 'bg-[#003087] text-white hover:bg-[#024089] active:bg-[#001f5c]',
      secondary: 'bg-[#ed6b06] text-white hover:bg-[#d65f04] active:bg-[#c64f01]',
      outline: 'border border-slate-300 bg-white hover:bg-slate-100 hover:text-slate-900 text-slate-700',
      ghost: 'hover:bg-slate-100 hover:text-slate-900 text-slate-600',
      destructive: 'bg-rose-600 text-white hover:bg-rose-700',
      link: 'text-[#003087] underline-offset-4 hover:underline p-0 h-auto'
    };

    const sizes: Record<ButtonSize, string> = {
      default: 'h-10 px-4 py-2',
      sm: 'h-8 rounded-md px-3 text-xs',
      lg: 'h-11 rounded-md px-6 text-base',
      icon: 'h-10 w-10'
    };

    return cn(base, variants[this._variant()], sizes[this._size()], this._class());
  });
}
