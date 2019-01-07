import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

// components
import { AudioTimelineComponent } from './components/audio-timeline';
import { ContentHeaderComponent } from './components/content-header';
import { IconComponent } from './components/icon';
import { IconButtonComponent } from './components/icon-button';
import { LoadingIndicatorComponent } from './components/loading-indicator';

// pipes
import { FormatIntegerPipe } from './pipes/format-integer';
import { FormatTimePipe } from './pipes/format-time';


@NgModule({
  declarations: [
    // components
    AudioTimelineComponent,
    ContentHeaderComponent,
    IconComponent,
    IconButtonComponent,
    LoadingIndicatorComponent,

    // pipes
    FormatIntegerPipe,
    FormatTimePipe
  ],
  exports: [
    // components
    AudioTimelineComponent,
    ContentHeaderComponent,
    IconComponent,
    IconButtonComponent,
    LoadingIndicatorComponent,

    // modules
    CommonModule,

    // pipes
    FormatIntegerPipe,
    FormatTimePipe
  ],
  imports: [
    CommonModule
  ]
})
export class SharedModule {}
