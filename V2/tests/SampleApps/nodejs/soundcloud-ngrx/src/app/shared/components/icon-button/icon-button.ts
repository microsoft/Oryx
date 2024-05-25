import { Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';


@Component({
  encapsulation: ViewEncapsulation.None,
  selector: 'icon-button',
  styleUrls: ['icon-button.scss'],
  template: `
    <button
      [attr.aria-label]="label"
      class="btn btn--icon btn--{{icon}} {{className}}"
      (click)="onClick.emit($event)"
      type="button">
      <icon [name]="icon"></icon>
    </button>
  `
})
export class IconButtonComponent {
  @Input() className: string;
  @Input() icon: string;
  @Input() label: string;

  @Output() onClick = new EventEmitter(false);
}
