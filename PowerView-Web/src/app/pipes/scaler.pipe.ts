import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'scaler'
})
export class ScalerPipe implements PipeTransform {

  transform(value: number): string {
    if (value == 0) return "";
    
    return Math.pow(10, value).toString();
  }

}
