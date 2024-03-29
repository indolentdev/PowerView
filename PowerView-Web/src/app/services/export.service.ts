import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { ExportSpec } from '../model/exportSpec';
import { ExportTitleSpec } from '../model/exportTitleSpec';
import { ExportSeriesDiffSet } from '../model/exportSeriesDiffSet';
import { ExportSeriesGaugeSet } from '../model/exportSeriesGaugeSet';
import { ExportCostBreakdown } from '../model/exportCostBreakdown';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  labels: "export/labels",
  hourly: "export/hourly",
  diffsHourly: "export/diffs/hourly",
  gaugesHourly: "export/gauges/hourly",

  costBreakdownTitles: "export/costbreakdowntitles",
  costBreakdownHourly: "export/costbreakdown/hourly"
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
    let params = new HttpParams()
      .set("from", exportSpec.from.toISOString())
      .set("to", exportSpec.to.add(1, 'days').toISOString());
    
    exportSpec.labels.forEach(label => {
      params = params.append("label", label);
    });

    return this.dataService.get<ExportSeriesDiffSet>(constLocal.diffsHourly, params, new ExportSeriesDiffSet);
  }

  public getGaugesExportHourly(exportSpec: ExportSpec): Observable<ExportSeriesGaugeSet> {
    let params = new HttpParams()
      .set("from", exportSpec.from.toISOString())
      .set("to", exportSpec.to.add(1, 'days').toISOString());
    
    exportSpec.labels.forEach(label => {
      params = params.append("label", label);
    });
    
    return this.dataService.get<ExportSeriesGaugeSet>(constLocal.gaugesHourly, params, new ExportSeriesGaugeSet);
  }

  public getCostBreakdownTitles(): Observable<string[]> {
    return this.dataService.get<string[]>(constLocal.costBreakdownTitles, null, []);
  }

  public getCostBreakdownExportHourly(exportSpec: ExportTitleSpec): Observable<ExportCostBreakdown> {
    let params = new HttpParams()
      .set("from", exportSpec.from.toISOString())
      .set("to", exportSpec.to.add(1, 'days').toISOString())
      .set("title", exportSpec.title);

    return this.dataService.get<ExportCostBreakdown>(constLocal.costBreakdownHourly, params, new ExportCostBreakdown);
  }

}
