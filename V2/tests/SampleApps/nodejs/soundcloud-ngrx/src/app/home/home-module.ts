import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// components
import { HomePageComponent } from './pages/home-page';

// modules
import { SharedModule } from '../shared';
import { TracklistsModule } from '../tracklists';

// routes
const routes: Routes = [
  {path: '', component: HomePageComponent}
];


@NgModule({
  declarations: [
    HomePageComponent
  ],
  imports: [
    RouterModule.forChild(routes),
    SharedModule,
    TracklistsModule
  ]
})
export class HomeModule {}
