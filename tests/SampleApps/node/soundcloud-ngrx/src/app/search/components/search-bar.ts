import { ChangeDetectionStrategy, Component, ElementRef, Input, OnChanges, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'search-bar',
  styleUrls: ['search-bar.scss'],
  template: `
    <div class="search-bar" [ngClass]="{'search-bar--open': open}" role="search">
      <form class="search-form" [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <input
          autocomplete="off"
          class="search-form__input"
          formControlName="search"
          maxlength="60"
          placeholder="Search Tracks"
          required
          type="text">
      </form>
    </div>
  `
})
export class SearchBarComponent implements OnChanges, OnInit {
  @Input() open = false;

  form: FormGroup;
  searchInput: FormControl;
  searchInputEl: HTMLInputElement;

  constructor(public el: ElementRef, public formBuilder: FormBuilder, public router: Router) {}

  ngOnChanges(changes: any): void {
    if (changes.open.currentValue) {
      this.searchInput.setValue('');
    }
  }

  ngOnInit(): void {
    this.searchInput = new FormControl();

    this.form = this.formBuilder.group({
      search: this.searchInput
    });

    this.searchInputEl = this.el.nativeElement.querySelector('input');

    this.el.nativeElement
      .querySelector('.search-bar')
      .addEventListener('transitionend', () => {
        if (this.open) {
          this.searchInputEl.focus();
        }
      }, false);
  }

  submit(): void {
    if (this.form.valid) {
      const value = this.searchInput.value.trim();
      if (value.length) {
        this.router.navigate(['/search', {q: value}]);
        this.searchInputEl.blur();
      }
    }
  }
}
