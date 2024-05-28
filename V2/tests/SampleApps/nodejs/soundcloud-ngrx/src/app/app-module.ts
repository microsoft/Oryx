import { APP_BASE_HREF } from '@angular/common';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';

// components
import { AppComponent } from './app';
import { AppHeaderComponent } from './app-header';

// modules
import { CoreModule } from './core';
import { HomeModule } from './home';
import { PlayerModule } from './player';
import { SearchModule } from './search';
import { SharedModule } from './shared';
import { TracklistsModule } from './tracklists';
import { UsersModule } from './users';
import { AppStateModule } from './app-state';


@NgModule({
  bootstrap: [
    AppComponent
  ],
  declarations: [
    AppComponent,
    AppHeaderComponent
  ],
  imports: [
    BrowserModule,
    RouterModule.forRoot([], {useHash: false}),
    AppStateModule,
    CoreModule,
    HomeModule,
    PlayerModule,
    SearchModule,
    SharedModule,
    TracklistsModule,
    UsersModule
  ],
  providers: [
    {provide: APP_BASE_HREF, useValue: '/'}
  ]
})
export class AppModule {}
