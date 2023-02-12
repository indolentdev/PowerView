import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { crudeData } from './crudeData.json.js'
import { crudeDataOnDate } from './crudeDataOnDate.json.js'
import { crudeDataMissingDays } from './crudeDataMissingDays.json.js'

export function crudeDataBackend(url: string, method: string, request: HttpRequest<any>): Observable<HttpEvent<any>> {

    if (url.endsWith('crude/missing-days') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: crudeDataMissingDays
            }));
            resp.complete();
        });
    }

    if (url.indexOf('data/crude/by/') > -1 && method === "GET") {
//        let crudeDataOnDateCopy = [{ ...crudeDataOnDate[0] }, { ...crudeDataOnDate[1] } ];
//        crudeDataOnDateCopy[0].value = Math.round((new Date().getTime() / 10) % 1000000);
//        crudeDataOnDateCopy[1].value = crudeDataOnDateCopy[0].value % 100000;
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: crudeDataOnDate
            }));
            resp.complete();
        });
    }

    if (url.endsWith('crude') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    label: crudeData.label,
                    totalCount: 888,
                    values: crudeData.values
                }
            }));
            resp.complete();
        });            
    }

    if (url.endsWith('devices/manualregisters') && method === "POST") {
        //        return throwError(new HttpErrorResponse({status:409}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

    if (url.indexOf('data/crude/values/') > -1 && method === "DELETE") {
        //        return throwError(new HttpErrorResponse({status:409}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

}