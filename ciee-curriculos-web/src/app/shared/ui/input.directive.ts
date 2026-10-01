import { Directive, Input, computed, signal } from '@angular/core';
import { cn } from './utils';

export type InputStatus = 'padrao' | 'extraido' | 'nao_encontrado' | 'manual';

@Directive({
  selector: '[hlmInput]',
  standalone: true,
  host: {
    '[class]': 'classes()'
  }
})
export class HlmInputDirective {
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
    // Totalmente livre de anéis e contornos externos (ring-0 e outline-none)
    const base = 'flex w-full rounded-md border px-3.5 py-2 text-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-slate-400 outline-none focus:outline-none focus-visible:outline-none ring-0 focus:ring-0 focus-visible:ring-0 disabled:cursor-not-allowed disabled:opacity-50';
    
    let variante = 'bg-white border-slate-300 text-slate-900 focus:border-slate-400 focus-visible:border-slate-400';
    if (this._error()) {
      variante = 'bg-rose-50/50 border-rose-400 text-slate-900 focus:border-rose-500 focus-visible:border-rose-500';
    } else if (this._status() === 'extraido') {
      variante = 'bg-emerald-50/70 border-emerald-300 text-emerald-950 placeholder:text-emerald-700/40 focus:border-emerald-400 focus-visible:border-emerald-400';
    } else if (this._status() === 'nao_encontrado') {
      variante = 'bg-amber-50/70 border-amber-300 text-amber-950 placeholder:text-amber-700/40 focus:border-amber-400 focus-visible:border-amber-400';
    }

    return cn(base, variante, this._class());
  });
}
