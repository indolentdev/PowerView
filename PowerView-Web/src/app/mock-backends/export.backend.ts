import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { exportDiffsHourly } from './exportDiffsHourly.json.js';
import { exportGaugesHourly } from './exportGaugesHourly.json.js';
import { exportHourly } from './exportHourly.json.js';

export function exportBackend(url: string, method: string, request: HttpRequest<any>): Observable<HttpEvent<any>> {

    if (url.endsWith('export/labels') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: ["yksi", "kaksi", "kolme"]
            }));
            resp.complete();
        });
    }

    if (url.endsWith('export/diffs/hourly') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: exportDiffsHourly
            }));
            resp.complete();
        });            
    }

    if (url.endsWith('export/gauges/hourly') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: exportGaugesHourly
            }));
            resp.complete();
        });            
    }

    if (url.endsWith('export/hourly') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: exportHourly
            }));
            resp.complete();
        });            
    }

}