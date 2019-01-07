import { FormatTimePipe } from './format-time';


describe('shared', () => {
  describe('FormatTimePipe', () => {
    let pipe: FormatTimePipe;

    beforeEach(() => {
      pipe = new FormatTimePipe();
    });

    it('should format hours from seconds', () => {
      expect(pipe.transform(3600)).toBe('1:00:00');
    });

    it('should format hours from milliseconds', () => {
      expect(pipe.transform(3600000, 'ms')).toBe('1:00:00');
    });

    it('should format minutes from seconds', () => {
      expect(pipe.transform(600)).toBe('10:00');
    });

    it('should format minutes from milliseconds', () => {
      expect(pipe.transform(600000, 'ms')).toBe('10:00');
    });

    it('should zero-pad single-digit minute', () => {
      expect(pipe.transform(60)).toBe('01:00');
    });

    it('should format seconds from seconds', () => {
      expect(pipe.transform(10)).toBe('00:10');
    });

    it('should format seconds from milliseconds', () => {
      expect(pipe.transform(10000, 'ms')).toBe('00:10');
    });

    it('should zero-pad single-digit second', () => {
      expect(pipe.transform(1)).toBe('00:01');
    });

    it('should format invalid `time` param', () => {
      expect(pipe.transform('1' as any)).toBe('00:00');
      expect(pipe.transform(0)).toBe('00:00');
      expect(pipe.transform(null)).toBe('00:00');
      expect(pipe.transform(undefined)).toBe('00:00');
    });
  });
});
