import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PlayerComponent } from './components/player/player.component';
import { SearchComponent } from './components/search/search.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, PlayerComponent, SearchComponent],
  template: `
    <div class="app">
      <header class="app-header">
        <h1>🎵 SoundCloud NgRx - Angular 18</h1>
        <p>Modern Angular 18 application with NgRx state management</p>
      </header>
      
      <main class="app-main">
        <app-search></app-search>
      </main>
      
      <footer class="app-footer">
        <app-player></app-player>
      </footer>
    </div>
  `,
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'soundcloud-ng18';
}
