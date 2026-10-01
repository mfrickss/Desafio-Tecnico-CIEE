import { Component, Directive, Input, computed, signal } from '@angular/core';
import { cn } from './utils';

@Component({
  selector: 'hlm-card',
  standalone: true,
  template: '<ng-content></ng-content>',
  host: {
    '[class]': 'classes()'
  }
})
export class HlmCardComponent {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('rounded-lg border border-slate-200 bg-white text-slate-900 shadow-xs block overflow-hidden', this._class()));
}

@Directive({
  selector: '[hlmCardHeader]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmCardHeaderDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('flex flex-col space-y-1.5 p-6 border-b border-slate-100', this._class()));
}

@Directive({
  selector: '[hlmCardTitle]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmCardTitleDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('font-bold tracking-tight text-[#001f5c] text-lg', this._class()));
}

@Directive({
  selector: '[hlmCardDescription]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmCardDescriptionDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('text-xs text-slate-500', this._class()));
}

@Directive({
  selector: '[hlmCardContent]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmCardContentDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('p-6', this._class()));
}

@Directive({
  selector: '[hlmCardFooter]',
  standalone: true,
  host: { '[class]': 'classes()' }
})
export class HlmCardFooterDirective {
  private readonly _class = signal<string>('');
  @Input() set class(value: string) { this._class.set(value); }
  protected readonly classes = computed(() => cn('flex items-center justify-end p-6 border-t border-slate-100 bg-slate-50/50', this._class()));
}
