export interface AppState {
    player: PlayerState;
    search: SearchState;
    tracklists: TracklistsState;
    tracks: TracksState;
    users: UsersState;
}

export interface PlayerState {
    currentTrackId: string | null;
    isPlaying: boolean;
    isLoading: boolean;
    volume: number;
    currentTime: number;
    duration: number;
}

export interface SearchState {
    query: string;
    results: Track[];
    isLoading: boolean;
    hasNextPage: boolean;
    nextUrl: string | null;
}

export interface TracklistsState {
    [key: string]: Tracklist;
}

export interface TracksState {
    [key: string]: Track;
}

export interface UsersState {
    [key: string]: User;
}

// Domain Models
export interface Track {
    id: string;
    title: string;
    description: string;
    permalink_url: string;
    artwork_url: string;
    waveform_url: string;
    created_at: string;
    duration: number;
    genre: string;
    original_format: string;
    playback_count: number;
    stream_url: string;
    streamable: boolean;
    tag_list: string;
    user_id: number;
    user: User;
}

export interface User {
    id: number;
    username: string;
    permalink_url: string;
    avatar_url: string;
    country: string;
    full_name: string;
    city: string;
}

export interface Tracklist {
    id: string;
    tracks: string[];
    isLoading: boolean;
    hasNextPage: boolean;
    nextUrl: string | null;
}
