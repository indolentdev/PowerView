import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { DiffValueSet } from '../model/diffValueSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  labels: "export/labels",
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
}
