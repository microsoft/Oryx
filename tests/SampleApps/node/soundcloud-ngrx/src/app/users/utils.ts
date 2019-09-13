function tracklistIdForUser(userId: number|string, resource: string): string {
  return `users/${userId}/${resource}`.toLowerCase();
}

export function tracklistIdForUserLikes(userId: number|string): string {
  return tracklistIdForUser(userId, 'likes');
}

export function tracklistIdForUserTracks(userId: number|string): string {
  return tracklistIdForUser(userId, 'tracks');
}
