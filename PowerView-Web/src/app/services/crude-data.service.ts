import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { CrudeValueSet } from '../model/crudeValueSet';
import { MissingDate } from '../model/missingDate';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  dataCrude: "data/crude",
  dataCrudeMissingDays: "data/crude/missing-days"
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

  public getDaysMissingCrudeValues(label: string): Observable<MissingDate[]> {
    var params = new HttpParams()
      .set("label", label);
    return this.dataService.get<MissingDate[]>(constLocal.dataCrudeMissingDays, params, []);
  }

}
