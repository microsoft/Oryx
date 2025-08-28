import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Track, User } from '../store/app.state';

@Injectable({
    providedIn: 'root'
})
export class SoundCloudService {
    private readonly http = inject(HttpClient);
    private readonly CLIENT_ID = 'CLIENT_ID_VALUE'; // Replace with actual client ID
    private readonly API_BASE = 'https://api.soundcloud.com';

    searchTracks(query: string, limit: number = 50): Observable<{ tracks: Track[]; hasNextPage: boolean; nextUrl: string | null }> {
        const params = new HttpParams()
            .set('q', query)
            .set('client_id', this.CLIENT_ID)
            .set('limit', limit.toString())
            .set('linked_partitioning', '1');

        return this.http.get<any>(`${this.API_BASE}/tracks`, { params }).pipe(
            map(response => ({
                tracks: response.collection || [],
                hasNextPage: !!response.next_href,
                nextUrl: response.next_href || null
            }))
        );
    }

    loadMoreTracks(nextUrl: string): Observable<{ tracks: Track[]; hasNextPage: boolean; nextUrl: string | null }> {
        const url = nextUrl.includes('client_id') ? nextUrl : `${nextUrl}&client_id=${this.CLIENT_ID}`;

        return this.http.get<any>(url).pipe(
            map(response => ({
                tracks: response.collection || [],
                hasNextPage: !!response.next_href,
                nextUrl: response.next_href || null
            }))
        );
    }

    getTrack(trackId: string): Observable<Track> {
        const params = new HttpParams().set('client_id', this.CLIENT_ID);
        return this.http.get<Track>(`${this.API_BASE}/tracks/${trackId}`, { params });
    }

    getUser(userId: string): Observable<User> {
        const params = new HttpParams().set('client_id', this.CLIENT_ID);
        return this.http.get<User>(`${this.API_BASE}/users/${userId}`, { params });
    }

    getStreamUrl(trackId: string): string {
        return `${this.API_BASE}/tracks/${trackId}/stream?client_id=${this.CLIENT_ID}`;
    }
}
