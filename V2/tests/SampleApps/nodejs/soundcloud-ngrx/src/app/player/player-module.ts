import { NgModule } from '@angular/core';
import { EffectsModule } from '@ngrx/effects';

// components
import { PlayerComponent } from './components/player';
import { PlayerControlsComponent } from './components/player-controls';
import { FormatVolumePipe } from './pipes/format-volume';

// modules
import { SharedModule } from 'app/shared';

// services
import { AUDIO_SOURCE_PROVIDER } from './audio-source';
import { PlayerActions } from './player-actions';
import { PlayerEffects } from './player-effects';
import { PlayerService } from './player-service';


@NgModule({
  declarations: [
    // components
    PlayerComponent,
    PlayerControlsComponent,

    // pipes
    FormatVolumePipe
  ],
  exports: [
    PlayerComponent
  ],
  imports: [
    EffectsModule.run(PlayerEffects),
    SharedModule
  ],
  providers: [
    AUDIO_SOURCE_PROVIDER,
    PlayerActions,
    PlayerService
  ]
})
export class PlayerModule {}
