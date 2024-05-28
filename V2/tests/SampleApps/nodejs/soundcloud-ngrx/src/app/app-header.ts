import { Component, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';


@Component({
  encapsulation: ViewEncapsulation.None,
  selector: 'app-header',
  styleUrls: ['app-header.scss'],
  template: `
    <header class="header">
      <div class="g-row g-cont">
        <div class="g-col">
          <h1 class="header__title"><a routerLink="/">SoundCloud • Angular2 NgRx</a></h1>
          <ul class="header__actions">
            <li>
              <icon-button icon="search-alt" (onClick)="toggleOpen()"></icon-button>
            </li>
            <li>
              <icon-button icon="soundcloud"></icon-button>
            </li>
            <li>
              <a class="link link--github" href="https://github.com/r-park/soundcloud-ngrx">
                <icon name="github"></icon>
              </a>
            </li>
          </ul>
        </div>
      </div>

      <div class="g-row g-cont">
        <div class="g-col">
          <search-bar [open]="open"></search-bar>
        </div>
      </div>
    </header>
  `
})
export class AppHeaderComponent {
  open = false;

  constructor(public router: Router) {
    this.router.events.subscribe(() => {
      if (this.open) this.toggleOpen();
    });
  }

  toggleOpen(): void {
    this.open = !this.open;
  }
}
