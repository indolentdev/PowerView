import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of, throwError, EMPTY } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

import { HistoryStatusSet } from '../model/historyStatusSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';
import { CrudeValue } from '../model/crudeValue';

const constLocal = {
  dataHistoryStatus: "data/history/status",
};

@Injectable({
  providedIn: 'root'
})
export class HistoryDataService {

  constructor(private dataService: DataService) {
  }

  public getHistoryStatus(): Observable<HistoryStatusSet> {
    var params = new HttpParams();
    return this.dataService.get<HistoryStatusSet>(constLocal.dataHistoryStatus, params, new HistoryStatusSet);
  }

}
