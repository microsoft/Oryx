import { Component, ElementRef, ViewChild, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Observable, Subject, of } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AppState, Track } from '../../store/app.state';
import * as PlayerActions from '../../store/player/player.actions';
import * as PlayerSelectors from '../../store/player/player.selectors';

@Component({
    selector: 'app-player',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="player" *ngIf="currentTrack$ | async as track">
      <div class="player__artwork">
        <img [src]="getArtworkUrl(track.artwork_url)" [alt]="track.title" />
      </div>
      
      <div class="player__details">
        <h3 class="player__title">{{ track.title }}</h3>
        <p class="player__artist">{{ track.user.username }}</p>
      </div>
      
      <div class="player__controls">
        <button 
          class="player__btn player__btn--play"
          (click)="togglePlayback()"
          [disabled]="isLoading$ | async">
          <span *ngIf="isPlaying$ | async">⏸</span>
          <span *ngIf="!(isPlaying$ | async)">▶</span>
        </button>
      </div>
      
      <div class="player__progress">
        <div class="player__time">{{ formatTime(currentTime()) }}</div>
        <div class="player__progress-bar" (click)="seek($event)">
          <div 
            class="player__progress-fill" 
            [style.width.%]="progress$ | async">
          </div>
        </div>
        <div class="player__time">{{ formatTime(duration()) }}</div>
      </div>
      
      <div class="player__volume">
        <input 
          type="range" 
          min="0" 
          max="1" 
          step="0.1"
          [value]="volume$ | async"
          (input)="setVolume($event)"
          class="player__volume-slider">
      </div>
      
      <audio 
        #audioElement
        [src]="getStreamUrl(track.id)"
        (loadedmetadata)="onLoadedMetadata()"
        (timeupdate)="onTimeUpdate()"
        (ended)="onEnded()">
      </audio>
    </div>
  `,
    styleUrls: ['./player.component.scss']
})
export class PlayerComponent implements OnInit, OnDestroy {
    @ViewChild('audioElement') audioElement!: ElementRef<HTMLAudioElement>;

    private readonly store = inject(Store<AppState>);
    private readonly destroy$ = new Subject<void>();

    // Modern Angular 18 signals for local state
    currentTime = signal(0);
    duration = signal(0);

    // NgRx selectors
    currentTrackId$ = this.store.select(PlayerSelectors.selectCurrentTrackId);
    currentTrack$: Observable<Track | null> = of(null); // Will be properly implemented with track selectors
    isPlaying$ = this.store.select(PlayerSelectors.selectIsPlaying);
    isLoading$ = this.store.select(PlayerSelectors.selectIsLoading);
    volume$ = this.store.select(PlayerSelectors.selectVolume);
    progress$ = this.store.select(PlayerSelectors.selectProgress);

    ngOnInit() {
        // Subscribe to playing state changes
        this.isPlaying$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(isPlaying => {
            if (this.audioElement) {
                if (isPlaying) {
                    this.audioElement.nativeElement.play();
                } else {
                    this.audioElement.nativeElement.pause();
                }
            }
        });

        // Subscribe to volume changes
        this.volume$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(volume => {
            if (this.audioElement) {
                this.audioElement.nativeElement.volume = volume;
            }
        });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    togglePlayback() {
        // In a real app, you'd get the current track ID from the store
        this.store.dispatch(PlayerActions.playTrack({ trackId: 'current-track-id' }));
    }

    seek(event: MouseEvent) {
        const progressBar = event.currentTarget as HTMLElement;
        const rect = progressBar.getBoundingClientRect();
        const percentage = (event.clientX - rect.left) / rect.width;
        const seekTime = percentage * this.duration();

        this.store.dispatch(PlayerActions.seekTo({ time: seekTime }));
        this.audioElement.nativeElement.currentTime = seekTime;
    }

    setVolume(event: Event) {
        const volume = parseFloat((event.target as HTMLInputElement).value);
        this.store.dispatch(PlayerActions.setVolume({ volume }));
    }

    onLoadedMetadata() {
        const audio = this.audioElement.nativeElement;
        this.duration.set(audio.duration);
        this.store.dispatch(PlayerActions.updateTime({
            currentTime: 0,
            duration: audio.duration
        }));
    }

    onTimeUpdate() {
        const audio = this.audioElement.nativeElement;
        this.currentTime.set(audio.currentTime);
        this.store.dispatch(PlayerActions.updateTime({
            currentTime: audio.currentTime,
            duration: audio.duration
        }));
    }

    onEnded() {
        this.store.dispatch(PlayerActions.pauseTrack());
    }

    getArtworkUrl(artworkUrl: string): string {
        if (!artworkUrl) return '/assets/default-artwork.png';
        return artworkUrl.replace('large.jpg', 't300x300.jpg');
    }

    getStreamUrl(trackId: string): string {
        return `https://api.soundcloud.com/tracks/${trackId}/stream?client_id=CLIENT_ID_VALUE`;
    }

    formatTime(seconds: number): string {
        if (!seconds || isNaN(seconds)) return '0:00';

        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = Math.floor(seconds % 60);
        return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
    }
}
