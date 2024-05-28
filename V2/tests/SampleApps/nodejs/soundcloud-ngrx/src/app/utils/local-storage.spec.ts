import { localStorageAdapter } from './local-storage';


describe('utils', () => {
  describe('localStorageAdapter', () => {
    let data;
    let storageKey;

    beforeEach(() => {
      data = {foo: "bar", baz: 123}; // tslint:disable-line:quotemark
      storageKey = 'soundcloud-ngrx:test';
    });

    afterEach(() => {
      localStorage.removeItem(storageKey);
    });


    it('should set serialized object into localStorage', () => {
      localStorageAdapter.setItem(storageKey, data);
      expect(localStorage.getItem(storageKey)).toEqual(
        JSON.stringify(data)
      );
    });

    it('should get serialized object from localStorage', () => {
      localStorage.setItem(storageKey, JSON.stringify(data));
      expect(localStorageAdapter.getItem(storageKey)).toEqual(data);
    });

    it('should return null if serialized object is not found in localStorage', () => {
      expect(localStorageAdapter.getItem(storageKey)).toEqual(null);
    });

    it('should remove key and corresponding value from localStorage', () => {
      localStorage.setItem(storageKey, JSON.stringify(data));
      localStorageAdapter.removeItem(storageKey);
      expect(localStorage.getItem(storageKey)).toBe(null);
    });
  });
});
