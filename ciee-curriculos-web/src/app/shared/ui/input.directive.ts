import { Directive, Input, computed, signal } from '@angular/core';
import { cn } from './utils';

export type InputStatus = 'padrao' | 'extraido' | 'nao_encontrado' | 'manual';

/**
 * Diretiva institucional CIEE para campos de formulário acessíveis (WCAG 2.1 AA).
 * Oferece anel de foco visível padronizado e feedback visual para campos extraídos via PDF.
 * Suporta o seletor institucional [cieeInput] e mantém retrocompatibilidade com [hlmInput].
 */
@Directive({
  selector: '[cieeInput]',
  standalone: true,
  host: {
    '[class]': 'classes()'
  }
})
export class CieeInputDirective {
  private readonly _class = signal<string>('');
  private readonly _error = signal<boolean>(false);
  private readonly _status = signal<InputStatus>('padrao');

  @Input()
  set class(value: string) {
    this._class.set(value);
  }

  @Input()
  set error(value: boolean) {
    this._error.set(value);
  }

  @Input()
  set status(value: InputStatus | undefined) {
    this._status.set(value ?? 'padrao');
  }

  protected readonly classes = computed(() => {
    const base = 'flex w-full rounded-md border px-3.5 py-2 text-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-slate-400 outline-none focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-1 disabled:cursor-not-allowed disabled:opacity-50';
    
    let variante = 'bg-white border-slate-300 text-slate-900 focus-visible:border-ciee-navy focus-visible:ring-ciee-navy';
    if (this._error()) {
      variante = 'bg-rose-50/50 border-rose-400 text-slate-900 focus-visible:border-rose-500 focus-visible:ring-rose-400';
    } else if (this._status() === 'extraido') {
      variante = 'bg-emerald-50/70 border-emerald-300 text-emerald-950 placeholder:text-emerald-700/40 focus-visible:border-emerald-500 focus-visible:ring-emerald-400';
    } else if (this._status() === 'nao_encontrado') {
      variante = 'bg-amber-50/70 border-amber-300 text-amber-950 placeholder:text-amber-700/40 focus-visible:border-amber-500 focus-visible:ring-amber-400';
    }

    return cn(base, variante, this._class());
  });
}

// Alias para compatibilidade com código existente
export { CieeInputDirective as HlmInputDirective };
