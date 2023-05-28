import { ScalerPipe } from './scaler.pipe';

describe('ScalerPipe', () => {
  it('create an instance', () => {
    const pipe = new ScalerPipe();
    expect(pipe).toBeTruthy();
  });

  it('Absent', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(0)).toBe("");
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
    expect(pipe.transform(-1)).toBe(("0.1"));
  });

  it('0.001', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(-3)).toBe(("0.001"));
  });

  it('0.00001', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(-5)).toBe(("0.00001"));
  });

  it('0.0000001', () => {
    const pipe = new ScalerPipe();
    expect(pipe.transform(-7)).toBe(("0.0000001"));
  });

});
