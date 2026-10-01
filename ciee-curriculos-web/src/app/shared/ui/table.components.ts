import { Component, Directive, Input, computed, signal } from '@angular/core';
import { cn } from './utils';

@Component({
  selector: 'hlm-table',
  standalone: true,
  template: '<div class=\"relative w-full overflow-auto\"><table class=\"w-full caption-bottom text-sm\"><ng-content></ng-content></table></div>',
  host: {
    '[class]': 'classes()'
  }
})
export class HlmTableComponent {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('w-full block rounded-lg border border-slate-200 overflow-hidden bg-white shadow-xs', this._class()));
}

@Directive({
  selector: '[hlmTableHeader]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmTableHeaderDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('border-b border-slate-200 bg-[#f8fafc]', this._class()));
}

@Directive({
  selector: '[hlmTableBody]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmTableBodyDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('divide-y divide-slate-100 bg-white', this._class()));
}

@Directive({
  selector: '[hlmTableRow]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmTableRowDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('border-b border-slate-100 transition-colors hover:bg-[#f0f6ff]/40 data-[state=selected]:bg-slate-100', this._class()));
}

@Directive({
  selector: '[hlmTableHead]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmTableHeadDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('h-10 px-4 text-left align-middle font-bold text-slate-600 uppercase text-xs tracking-wider', this._class()));
}

@Directive({
  selector: '[hlmTableCell]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmTableCellDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('p-4 align-middle text-slate-800', this._class()));
}