import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { EffectsModule } from '@ngrx/effects';

// components
import { SearchBarComponent } from './components/search-bar';
import { SearchPageComponent } from './pages/search-page';

// modules
import { SharedModule } from '../shared';
import { TracklistsModule } from '../tracklists';

// services
import { SearchActions } from './search-actions';
import { SearchEffects } from './search-effects';
import { SearchService } from './search-service';

// routes
const routes: Routes = [
  {path: 'search', component: SearchPageComponent}
];


@NgModule({
  declarations: [
    SearchBarComponent,
    SearchPageComponent
  ],
  exports: [
    SearchBarComponent
  ],
  imports: [
    EffectsModule.run(SearchEffects),
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    SharedModule,
    TracklistsModule
  ],
  providers: [
    SearchActions,
    SearchService
  ]
})
export class SearchModule {}
