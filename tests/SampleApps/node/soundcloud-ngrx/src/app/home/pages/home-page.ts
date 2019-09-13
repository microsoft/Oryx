import { Component } from '@angular/core';
import { TracklistService } from 'app/tracklists';


@Component({
  template: `
    <section>
      <content-header 
        [section]="section" 
        [title]="title"></content-header>

      <tracklist [layout]="layout"></tracklist>
    </section>
  `
})
export class HomePageComponent {
  layout = 'compact';
  section = 'Spotlight';
  title = 'Featured Tracks';

  constructor(public tracklist: TracklistService) {
    tracklist.loadFeaturedTracks();
  }
}
