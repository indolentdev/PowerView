import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { ExportSpec } from '../model/exportSpec';
import { ExportSeriesSet } from '../model/exportSeriesSet';
import { ExportSeriesDiffSet } from '../model/exportSeriesDiffSet';
import { ExportSeriesGaugeSet } from '../model/exportSeriesGaugeSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  labels: "export/labels",
  hourly: "export/hourly",
  diffsHourly: "export/diffs/hourly",
  gaugesHourly: "export/gauges/hourly"
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

  public getDiffsExportHourly(exportSpec: ExportSpec): Observable<ExportSeriesDiffSet> {
    var params = new HttpParams()
      .set("labels", exportSpec.labels.join())
      .set("from", exportSpec.from.toISOString())
      .set("to", exportSpec.to.add(1, 'days').toISOString());
    return this.dataService.get<ExportSeriesDiffSet>(constLocal.diffsHourly, params, new ExportSeriesDiffSet);
  }

  public getGaugesExportHourly(exportSpec: ExportSpec): Observable<ExportSeriesGaugeSet> {
    var params = new HttpParams()
      .set("labels", exportSpec.labels.join())
      .set("from", exportSpec.from.toISOString())
      .set("to", exportSpec.to.add(1, 'days').toISOString());
    return this.dataService.get<ExportSeriesGaugeSet>(constLocal.gaugesHourly, params, new ExportSeriesGaugeSet);
  }

  public getHourlyExport(labels: string[], from: Moment, to: Moment): Observable<ExportSeriesSet> {
    var params = new HttpParams()
      .set("labels", labels.join())
      .set("from", from.toISOString())
      .set("to", to.toISOString());
    return this.dataService.get<ExportSeriesSet>(constLocal.hourly, params, new ExportSeriesSet);
  }
}
