import { testUtils } from 'app/utils/test';
import { FormatVolumePipe } from './format-volume';


describe('player', () => {
  describe('FormatVolumePipe', () => {
    it('should format volume values', () => {
      let pipe = new FormatVolumePipe();

      testUtils.getVolumes().forEach(({input, display}) => {
        expect(pipe.transform(input)).toBe(display);
      });
    });
  });
});
