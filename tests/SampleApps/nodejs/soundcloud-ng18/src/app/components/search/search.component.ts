import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

import { AppState, Track } from '../../store/app.state';
import * as SearchActions from '../../store/search/search.actions';
import * as PlayerActions from '../../store/player/player.actions';

@Component({
    selector: 'app-search',
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
    <div class="search">
      <div class="search__header">
        <h2>Search SoundCloud</h2>
        <div class="search__form">
          <input 
            type="text" 
            [(ngModel)]="searchQuery"
            (keyup.enter)="search()"
            placeholder="Search for tracks..."
            class="search__input">
          <button 
            (click)="search()"
            [disabled]="!searchQuery.trim()"
            class="search__btn">
            Search
          </button>
        </div>
      </div>

      <div class="search__results" *ngIf="searchResults$ | async as results">
        <div class="track-list">
          <div 
            class="track-item" 
            *ngFor="let track of results.results"
            (click)="playTrack(track)">
            
            <div class="track-item__artwork">
              <img [src]="getArtworkUrl(track.artwork_url)" [alt]="track.title">
              <div class="track-item__play-overlay">
                <span class="play-icon">▶</span>
              </div>
            </div>
            
            <div class="track-item__info">
              <h3 class="track-item__title">{{ track.title }}</h3>
              <p class="track-item__artist">{{ track.user.username }}</p>
              <div class="track-item__meta">
                <span class="track-item__duration">{{ formatDuration(track.duration) }}</span>
                <span class="track-item__plays">{{ formatPlayCount(track.playback_count) }} plays</span>
              </div>
            </div>
            
            <div class="track-item__actions">
              <button 
                class="track-item__btn"
                (click)="$event.stopPropagation(); playTrack(track)"
                title="Play track">
                ▶
              </button>
            </div>
          </div>
        </div>

        <div class="search__loading" *ngIf="isLoading$ | async">
          <div class="loading-spinner"></div>
          <p>Searching...</p>
        </div>

        <div class="search__load-more" *ngIf="(hasNextPage$ | async) && !(isLoading$ | async)">
          <button 
            class="search__btn search__btn--secondary"
            (click)="loadMore()">
            Load More Tracks
          </button>
        </div>

        <div class="search__empty" *ngIf="!results.results.length && !(isLoading$ | async)">
          <p>No tracks found. Try a different search term.</p>
        </div>
      </div>
    </div>
  `,
    styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit {
    private readonly store = inject(Store<AppState>);

    searchQuery = '';

    // NgRx selectors
    searchResults$ = this.store.select(state => state.search);
    isLoading$ = this.store.select(state => state.search.isLoading);
    hasNextPage$ = this.store.select(state => state.search.hasNextPage);

    ngOnInit() {
        // Clear previous search results
        this.store.dispatch(SearchActions.clearSearchResults());
    }

    search() {
        const query = this.searchQuery.trim();
        if (query) {
            this.store.dispatch(SearchActions.searchTracks({ query }));
        }
    }

    loadMore() {
        this.store.dispatch(SearchActions.loadMoreTracks());
    }

    playTrack(track: Track) {
        this.store.dispatch(PlayerActions.playTrack({ trackId: track.id }));
    }

    getArtworkUrl(artworkUrl: string): string {
        if (!artworkUrl) return '/assets/default-artwork.png';
        return artworkUrl.replace('large.jpg', 't300x300.jpg');
    }

    formatDuration(milliseconds: number): string {
        const seconds = Math.floor(milliseconds / 1000);
        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = seconds % 60;
        return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
    }

    formatPlayCount(count: number): string {
        if (count >= 1000000) {
            return `${(count / 1000000).toFixed(1)}M`;
        } else if (count >= 1000) {
            return `${(count / 1000).toFixed(1)}K`;
        }
        return count.toString();
    }
}
