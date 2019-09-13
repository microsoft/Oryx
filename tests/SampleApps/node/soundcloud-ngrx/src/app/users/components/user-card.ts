import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { IUser } from '../models';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'user-card',
  styleUrls: ['user-card.scss'],
  template: `
    <article class="user-card">
      <div class="g-row g-cont" *ngIf="user">
        <div class="g-col">
          <h1 class="user-card__title">{{user.username}}</h1>
        </div>
      </div>

      <div class="g-row g-cont user-stats" *ngIf="user">
        <div class="g-col xs-1/2 sm-1/4">
          <a class="user-stats__label"
            [class.active]="resource === 'tracks'"
            [routerLink]="['/users', user.id, 'tracks']">Tracks</a>
          <div class="user-stats__value">{{user.trackCount | formatInteger}}</div>
        </div>

        <div class="g-col xs-1/2 sm-1/4">
          <a class="user-stats__label"
            [class.active]="resource === 'likes'"
            [routerLink]="['/users', user.id, 'likes']">Likes</a>
          <div class="user-stats__value">{{user.likesCount | formatInteger}}</div>
        </div>

        <div class="g-col xs-hide sm-1/4">
          <div class="user-stats__label">Followers</div>
          <div class="user-stats__value">{{user.followersCount | formatInteger}}</div>
        </div>

        <div class="g-col xs-hide sm-1/4">
          <div class="user-stats__label">Following</div>
          <div class="user-stats__value">{{user.followingsCount | formatInteger}}</div>
        </div>
      </div>
    </article>
  `
})
export class UserCardComponent {
  @Input() resource = 'tracks';
  @Input() user: IUser;
}
