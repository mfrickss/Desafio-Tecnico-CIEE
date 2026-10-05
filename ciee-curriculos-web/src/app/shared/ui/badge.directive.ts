import { Directive, Input, computed, signal } from '@angular/core';
import { cn } from './utils';

export type BadgeVariant = 'default' | 'secondary' | 'outline' | 'success' | 'warning' | 'destructive';

/**
 * Diretiva institucional CIEE para estilização de badges/etiquetas.
 * Suporta o seletor institucional [cieeBadge] e mantém retrocompatibilidade com [hlmBadge].
 */
@Directive({
  selector: '[cieeBadge]',
  standalone: true,
  host: {
    '[class]': 'classes()'
  }
})
export class CieeBadgeDirective {
  private readonly _variant = signal<BadgeVariant>('default');
  private readonly _class = signal<string>('');

  @Input()
  set variant(value: BadgeVariant) {
    this._variant.set(value);
  }

  @Input()
  set class(value: string) {
    this._class.set(value);
  }

  protected readonly classes = computed(() => {
    const base = 'inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors outline-none';

    const variants: Record<BadgeVariant, string> = {
      default: 'border-transparent bg-ciee-navy text-white',
      secondary: 'border-slate-200 bg-slate-100 text-slate-700',
      outline: 'border-slate-300 bg-white text-slate-700',
      success: 'border-emerald-200 bg-emerald-50 text-emerald-800',
      warning: 'border-amber-200 bg-amber-50 text-amber-800',
      destructive: 'border-transparent bg-rose-500 text-white'
    };

    return cn(base, variants[this._variant()], this._class());
  });
}

// Alias para compatibilidade com código existente
export { CieeBadgeDirective as HlmBadgeDirective };
