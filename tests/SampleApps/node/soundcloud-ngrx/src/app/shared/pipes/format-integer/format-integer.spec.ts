import { FormatIntegerPipe } from './format-integer';


describe('shared', () => {
  describe('FormatIntegerPipe', () => {
    let pipe;

    beforeEach(() => {
      pipe = new FormatIntegerPipe();
    });


    it('should insert comma to separate groups of thousands', () => {
      expect(pipe.transform(1000)).toBe('1,000');
      expect(pipe.transform(10000)).toBe('10,000');
      expect(pipe.transform(100000)).toBe('100,000');
      expect(pipe.transform(1000000)).toBe('1,000,000');
    });

    it('should return unmodified integer if provided integer is less than 1000', () => {
      expect(pipe.transform(0)).toBe(0);
      expect(pipe.transform(1)).toBe(1);
      expect(pipe.transform(10)).toBe(10);
      expect(pipe.transform(100)).toBe(100);
    });
  });
});
