import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of } from 'rxjs';
import { DiffValueSet } from '../model/diffValueSet';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';

const constLocal = {
  diff: "diff",
};

@Injectable({
  providedIn: 'root'
})
export class DiffService {

  constructor(private dataService: DataService) { 
  }

  public getDiffValues(from: Moment, to: Moment): Observable<DiffValueSet> {
    var params = new HttpParams()
      .set("from", from.toISOString())
      .set("to", to.toISOString());
    return this.dataService.get<DiffValueSet>(constLocal.diff, params, new DiffValueSet);
  }
}
