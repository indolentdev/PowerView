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
  dataCrudeOnDate: "data/crude/by",
  dataCrudeMissingDays: "data/crude/missing-days",
  dataCrudeValues: "data/crude/values",

  devicesManualregisters: "devices/manualregisters"
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

  public addManualReading(label: string, crudeValue: CrudeValue): Observable<any> {
    let body = { ...crudeValue };
    body["label"] = label;
    return this.dataService.post(constLocal.devicesManualregisters, body)
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

  public deleteCrudeValue(label: string, timestamp: string, obisCode: string): Observable<any> {
    return this.dataService.delete(`${constLocal.dataCrudeValues}/${encodeURIComponent(label)}/${encodeURIComponent(timestamp)}/${encodeURIComponent(obisCode)}`)
      .pipe(catchError(error => {
        return throwError(this.convertToDeleteCrudeValueError(error));
      }));
  }

  private convertToDeleteCrudeValueError(error: any): DeleteCrudeValueError {
    if (!(error instanceof HttpErrorResponse)) {
      return DeleteCrudeValueError.UnspecifiedError;
    }

    var httpErrorResponse = error as HttpErrorResponse;
    switch (httpErrorResponse.status) {
      default:
        return DeleteCrudeValueError.UnspecifiedError;
    }
  }

  public getCrudeValuesOnDate(label: string, timestamp: string): Observable<CrudeValue[]> {
    return this.dataService.get<CrudeValue[]>(`${constLocal.dataCrudeOnDate}/${encodeURIComponent(label)}/${encodeURIComponent(timestamp)}`, null, []);
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

export enum DeleteCrudeValueError {
  UnspecifiedError = "UnspecifiedError"
}
