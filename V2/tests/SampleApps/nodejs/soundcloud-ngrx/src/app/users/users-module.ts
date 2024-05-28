import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EffectsModule } from '@ngrx/effects';

// components
import { UserCardComponent } from './components/user-card';
import { UserPageComponent } from './pages/user-page';

// modules
import { SharedModule } from '../shared';
import { TracklistsModule } from '../tracklists';

// services
import { UserActions } from './user-actions';
import { UserEffects } from './user-effects';
import { UserService } from './user-service';

// routes
const routes: Routes = [
  {path: 'users/:id/:resource', component: UserPageComponent}
];


@NgModule({
  declarations: [
    UserCardComponent,
    UserPageComponent
  ],
  imports: [
    RouterModule.forChild(routes),
    SharedModule,
    EffectsModule.run(UserEffects),
    TracklistsModule
  ],
  providers: [
    UserActions,
    UserService
  ]
})
export class UsersModule {}
