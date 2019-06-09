import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { GaugeValueGroupSet } from '../model/gaugeValueGroupSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  guagesLatest: "gauges/latest",
  guagesCustom: "gauges/custom"
};

@Injectable({
  providedIn: 'root'
})
export class GaugesService {

  constructor(private dataService: DataService) {
  }

  public getLatestGaugeValues(): Observable<GaugeValueGroupSet> {
    return this.dataService.get<GaugeValueGroupSet>(constLocal.guagesLatest, undefined, new GaugeValueGroupSet);
  }

  public getCustomGaugeValues(timestamp: Moment): Observable<GaugeValueGroupSet> {
    var params = new HttpParams().set("timestamp", timestamp.toISOString());
    return this.dataService.get<GaugeValueGroupSet>(constLocal.guagesCustom, params, new GaugeValueGroupSet);
  }  
}
