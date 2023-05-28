import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'scaler'
})
export class ScalerPipe implements PipeTransform {

  transform(value: number): string {
    if (value == 0) return "";

    if (value > 0) {
      return Math.pow(10, value).toString();
    }

    var pow = Math.pow(10, value);
    var decimals = this.getDecimalPlaces(pow, 10);
    return pow.toFixed(decimals);
  }

  private getDecimalPlaces(x, watchdog) {
    x = Math.abs(x);
    watchdog = watchdog || 20;
    var i = 0;
    while (x % 1 > 0 && i < watchdog) {
      i++;
      x = x * 10;
    }
    return i;
  }
}
