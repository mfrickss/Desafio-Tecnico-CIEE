import { Directive, Input, computed, signal } from '@angular/core';
import { cn } from './utils';

export type ButtonVariant = 'default' | 'secondary' | 'outline' | 'ghost' | 'destructive' | 'link';
export type ButtonSize = 'default' | 'sm' | 'lg' | 'icon';

/**
 * Diretiva institucional CIEE para estilização de botões acessíveis e padronizados.
 * Suporta o seletor institucional [cieeBtn] e mantém retrocompatibilidade com [hlmBtn].
 */
@Directive({
  selector: '[cieeBtn]',
  standalone: true,
  host: {
    '[class]': 'classes()'
  }
})
export class CieeButtonDirective {
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
    const base = 'inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-semibold transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-ciee-navy focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50 select-none cursor-pointer';

    const variants: Record<ButtonVariant, string> = {
      default: 'bg-ciee-navy text-white hover:bg-ciee-blue active:bg-ciee-dark',
      secondary: 'bg-ciee-orange text-white hover:opacity-95 active:bg-ciee-orangeFocus',
      outline: 'border border-slate-300 bg-white hover:bg-slate-100 hover:text-slate-900 text-slate-700',
      ghost: 'hover:bg-slate-100 hover:text-slate-900 text-slate-600',
      destructive: 'bg-rose-600 text-white hover:bg-rose-700 active:bg-rose-800',
      link: 'text-ciee-navy underline-offset-4 hover:underline p-0 h-auto'
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

// Alias para preservar compatibilidade de importação existente
export { CieeButtonDirective as HlmButtonDirective };
