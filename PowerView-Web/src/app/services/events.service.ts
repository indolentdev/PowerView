import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { EventSet } from '../model/eventSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  events: "events"
};

@Injectable({
  providedIn: 'root'
})
export class EventsService {

  constructor(private dataService: DataService) {
  }

  public getLatestEvents(): Observable<EventSet> {
    return this.dataService.get<EventSet>(constLocal.events, undefined, new EventSet);
  }
  
/*
  public getCustomGaugeValues(timestamp: Moment): Observable<GaugeValueGroupSet> {
    var params = new HttpParams().set("timestamp", timestamp.toISOString());
    return this.dataService.get<GaugeValueGroupSet>(constLocal.guagesCustom, params, new GaugeValueGroupSet);
  }  
*/  
}
