import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export type IconType = 
  | 'check' 
  | 'alert' 
  | 'spinner' 
  | 'pdf' 
  | 'search' 
  | 'arrow-left' 
  | 'close' 
  | 'plus' 
  | 'users' 
  | 'info' 
  | 'error-triangle';

@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './icon.component.html',
  host: {
    'class': 'inline-flex items-center justify-center'
  }
})
export class AppIconComponent {
  readonly name = signal<IconType>('check');
  private readonly _class = signal<string>('w-4 h-4');

  @Input({ required: true, alias: 'name' })
  set nameInput(value: IconType) {
    this.name.set(value);
  }

  @Input()
  set class(value: string) {
    if (value) this._class.set(value);
  }

  protected readonly svgClasses = computed(() => this._class());
  protected readonly spinnerClasses = computed(() => 'border-2 border-current border-t-transparent rounded-full animate-spin ' + this._class());
}
