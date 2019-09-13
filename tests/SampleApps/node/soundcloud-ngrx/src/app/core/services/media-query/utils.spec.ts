import { em, getMedia } from './utils';


describe('media-query', () => {
  describe('utils', () => {
    describe('em()', () => {
      it('should convert provided number to string em value', () => {
        let expectedEmValue = `${100 / 16}em`;
        expect(em(100)).toBe(expectedEmValue);
      });

      it('should return zero if provided number is zero', () => {
        let expectedEmValue = 0;
        expect(em(0)).toBe(expectedEmValue);
      });
    });


    describe('getMedia()', () => {
      const id = 'large';

      it('should create media query string with provided `type`', () => {
        let media = getMedia({id, type: 'print'});
        expect(media).toBe('print');
      });

      it('should create media query string with default `type`', () => {
        let media = getMedia({id});
        expect(media).toBe('screen');
      });

      it('should create media query string with provided `maxWidth`', () => {
        let media = getMedia({id, maxWidth: 320});
        expect(media).toBe('screen and (max-width: 20em)');
      });

      it('should create media query string with provided `minWidth`', () => {
        let media = getMedia({id, minWidth: 320});
        expect(media).toBe('screen and (min-width: 20em)');
      });

      it('should create media query string with provided `orientation`', () => {
        let media = getMedia({id, orientation: 'portrait'});
        expect(media).toBe('screen and (orientation: portrait)');

        media = getMedia({id, orientation: 'landscape'});
        expect(media).toBe('screen and (orientation: landscape)');
      });

      it('should create media query string when multiple features are provided', () => {
        let expectedValue = 'all and (min-width: 20em) and (max-width: 40em) and (orientation: portrait)';

        let media = getMedia({
          id,
          maxWidth: 640,
          minWidth: 320,
          orientation: 'portrait',
          type: 'all'
        });

        expect(media).toBe(expectedValue);
      });
    });
  });
});
