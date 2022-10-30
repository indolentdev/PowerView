import { Injectable } from '@angular/core';
import { DataService } from './data.service';
import { Observable, of, throwError, EMPTY } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

import { CrudeValueSet } from '../model/crudeValueSet';
import { MissingDate } from '../model/missingDate';

import { Moment } from 'moment'
import * as moment from 'moment';
import { HttpParams } from '@angular/common/http';
import { CrudeValue } from '../model/crudeValue';

const constLocal = {
  dataCrude: "data/crude",
  dataCrudeOnDate: "data/crude/by/",
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

  public addCrudeValue(label: string, crudeValue: CrudeValue): Observable<any> {
    let body = { ...crudeValue };
    body["label"] = label;
    return this.dataService.post(constLocal.dataCrude, body)
      .pipe(catchError(error => {
        return throwError(this.convertToAddCrudeValueError(error));
      }));
  }

  private convertToAddCrudeValueError(error: any): AddCrudeValueError {
    if (!(error instanceof HttpErrorResponse)) {
      return AddCrudeValueError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
//      case 400:
//        return AddCrudeValueError.RequestContentIncomplete;
      case 409:
        return AddCrudeValueError.RequestContentDuplicate;
      default:
        return AddCrudeValueError.UnspecifiedError;
    }
  }

  public getCrudeValuesOnDate(label: string, timestamp: string): Observable<CrudeValue[]> {
    return this.dataService.get<CrudeValue[]>(`${constLocal.dataCrudeOnDate}/${label}/${timestamp}`, null, []);
  }

  public getDaysMissingCrudeValues(label: string): Observable<MissingDate[]> {
    var params = new HttpParams()
      .set("label", label);
    return this.dataService.get<MissingDate[]>(constLocal.dataCrudeMissingDays, params, []);
  }

}

export enum AddCrudeValueError {
  UnspecifiedError = "UnspecifiedError",
//  RequestContentIncomplete = "RequestContentIncomplete",
  RequestContentDuplicate = "RequestContentDuplicate"
}

