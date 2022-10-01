import { Injectable } from '@angular/core';
import { NGXLogger } from 'ngx-logger';
import { TranslateService, TranslatePipe } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class ObisService {

  constructor(private log: NGXLogger, private translateService: TranslateService) { 
  }

  // Aids in defining a property which is application transalted register name
  // so that it is easy to use for angular material tables e.g. using sorting.
  public AddRegisterProperty(items: any[], obisCodeName: string = "obisCode", registerName: string = "register"): any[] {
    let rows = [];
    for (let item of items) {
      let row: any = item;
      rows.push(row);

      if (!item.hasOwnProperty(obisCodeName)) {
        this.log.warn(`Unable to add ${registerName} property. Object is missing ${obisCodeName} property.`, item);
        continue;
      }

      let register = item[obisCodeName];
      row[registerName] = register; // Ensure the register property is set to something..
      this.translateService.get("serie." + item[obisCodeName]).subscribe(register => {
        row[registerName] = register;
      });
    }
    return rows;
  }

  // Aids in defining a property which is combined of the user label and application transalted register name
  // so that it is easy to use for angular material tables e.g. using sorting.
  public AddSerieProperty(items: any[], labelName: string = "label", obisCodeName: string = "obisCode", serieName: string = "serie"):any[] {
    let rows = [];
    for (let item of items) {
      let row:any = item;
      rows.push(row);
      
      if (!item.hasOwnProperty(labelName) || !item.hasOwnProperty(obisCodeName)) {
        this.log.warn(`Unable to add ${serieName} property. Object is missing ${labelName} and ${obisCodeName} properties.`, item);
        continue;
      }

      let label = item[labelName];
      let serie = label + " " + item[obisCodeName];
      row[serieName] = serie; // Ensure the serie property is set to something..
      this.translateService.get("serie." + item[obisCodeName]).subscribe(register => {
        let serie = `${label} ${register}`;
        row[serieName] = serie;
      });
    }
    return rows;
  }

}
