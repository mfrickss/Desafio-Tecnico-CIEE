import { Component, Directive, Input, computed, signal } from '@angular/core';
import { cn } from './utils';

/**
 * Componente institucional CIEE: Card de container.
 * Suporta o seletor institucional ciee-card e mantém retrocompatibilidade com hlm-card.
 */
@Component({
  selector: 'ciee-card',
  standalone: true,
  template: '<ng-content></ng-content>',
  host: {
    '[class]': 'classes()'
  }
})
export class CieeCardComponent {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('rounded-lg border border-slate-200 bg-white text-slate-900 shadow-xs block overflow-hidden', this._class()));
}

@Directive({
  selector: '[cieeCardHeader]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class CieeCardHeaderDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('flex flex-col space-y-1.5 p-6 border-b border-slate-100', this._class()));
}

@Directive({
  selector: '[cieeCardTitle]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class CieeCardTitleDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('font-bold tracking-tight text-ciee-dark text-lg', this._class()));
}

@Directive({
  selector: '[cieeCardDescription]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class CieeCardDescriptionDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('text-xs text-slate-500', this._class()));
}

@Directive({
  selector: '[cieeCardContent]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class CieeCardContentDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('p-6', this._class()));
}

@Directive({
  selector: '[cieeCardFooter]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class CieeCardFooterDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('flex items-center justify-end p-6 border-t border-slate-100 bg-slate-50/50', this._class()));
}
