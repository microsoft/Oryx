import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { EffectsModule } from '@ngrx/effects';

// components
import { TrackCardComponent } from './components/track-card';
import { TracklistComponent } from './components/tracklist';
import { TracklistItemsComponent } from './components/tracklist-items';
import { WaveformComponent } from './components/waveform';
import { WaveformTimelineComponent } from './components/waveform-timeline';

// modules
import { SharedModule } from '../shared';

// services
import { TracklistActions } from './tracklist-actions';
import { TracklistEffects } from './tracklist-effects';
import { TracklistService } from './tracklist-service';


@NgModule({
  declarations: [
    TrackCardComponent,
    TracklistComponent,
    TracklistItemsComponent,
    WaveformComponent,
    WaveformTimelineComponent
  ],
  exports: [
    TracklistComponent
  ],
  imports: [
    RouterModule,
    SharedModule,
    EffectsModule.run(TracklistEffects)
  ],
  providers: [
    TracklistActions,
    TracklistService
  ]
})
export class TracklistsModule {}
