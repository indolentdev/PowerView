import { ScalerPipe } from './scaler.pipe';

describe('ScalerPipe', () => {
  it('create an instance', () => {
    const pipe = new ScalerPipe();
    expect(pipe).toBeTruthy();
  });

  it('1', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(0)).toBe("1");
  });

  it('10', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(1)).toBe("10");
  });

  it('1000', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(3)).toBe("1000");
  });

  it('0.1', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(-1)).toBe((0.1).toString());
  });

  it('0.001', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(-3)).toBe((0.001).toString());
  });

});
