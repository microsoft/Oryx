export const localStorageAdapter = {
  getItem(key: string): any {
    return JSON.parse(localStorage.getItem(key));
  },

  setItem(key: string, value: any): void {
    localStorage.setItem(key, JSON.stringify(value));
  },

  removeItem(key: string): void {
    localStorage.removeItem(key);
  }
};
