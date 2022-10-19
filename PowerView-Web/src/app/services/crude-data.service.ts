import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { CrudeValueSet } from '../model/crudeValueSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  dataCrude: "data/crude"
};

@Injectable({
  providedIn: 'root'
})
export class CrudeDataService {

  constructor(private dataService: DataService) {
  }

  public getCrudeValues(label: string, from: Moment): Observable<CrudeValueSet> {
    var params = new HttpParams()
      .set("label", label)
      .set("from", from.toISOString());
    return this.dataService.get<CrudeValueSet>(constLocal.dataCrude, params, new CrudeValueSet);
  }
}
