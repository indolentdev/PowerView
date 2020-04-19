import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { ExportSeriesSet } from '../model/exportSeriesSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  labels: "export/labels",
  hourly: "export/hourly"
};

@Injectable({
  providedIn: 'root'
})
export class ExportService {

  constructor(private dataService: DataService) { 
  }

  public getLabels(): Observable<string[]> {
    return this.dataService.get<string[]>(constLocal.labels, null, []);
  }

  public getHourlyExport(labels: string[], from: Moment, to: Moment): Observable<ExportSeriesSet> {
    var params = new HttpParams()
      .set("labels", labels.join())
      .set("from", from.toISOString())
      .set("to", to.toISOString());
    return this.dataService.get<ExportSeriesSet>(constLocal.hourly, params, new ExportSeriesSet);
  }
}
