import { Directive, ElementRef, Input, OnInit, inject } from '@angular/core';

@Directive({
  selector: '[appGsapFadeIn]',
  standalone: true
})
export class GsapFadeInDirective implements OnInit {
  private readonly el = inject(ElementRef);

  @Input() delay: number = 0;
  @Input() duration: number = 0.4;
  @Input() yOffset: number = 10;

  ngOnInit() {
    const native = this.el.nativeElement as HTMLElement;
    native.style.opacity = '0';
    native.style.transform = 'translateY(' + this.yOffset + 'px)';
    native.style.transition = 'opacity ' + this.duration + 's ease, transform ' + this.duration + 's ease';

    setTimeout(() => {
      native.style.opacity = '1';
      native.style.transform = 'translateY(0)';
    }, Math.max(10, this.delay * 1000));
  }
}
