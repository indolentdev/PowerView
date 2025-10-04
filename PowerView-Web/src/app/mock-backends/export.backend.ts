import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { exportDiffsQuarterly } from './exportDiffsQuarterly.json.js';
import { exportDiffsHourly } from './exportDiffsHourly.json.js';
import { exportGaugesQuarterly } from './exportGaugesQuarterly.json.js';
import { exportGaugesHourly } from './exportGaugesHourly.json.js';
import { exportCostBreakdownQuarterly } from './exportCostBreakdownQuarterly.json.js';
import { exportCostBreakdownHourly } from './exportCostBreakdownHourly.json.js';

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

    if (url.endsWith('export/diffs/quarterly') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: exportDiffsQuarterly
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

    if (url.endsWith('export/gauges/quarterly') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: exportGaugesQuarterly
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

    if (url.endsWith('export/costbreakdowntitles') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: ["vi", "vil", "ha", "winerbrød"]
            }));
            resp.complete();
        });
    }

    if (url.endsWith('export/costbreakdown/quarterly') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: exportCostBreakdownQuarterly
            }));
            resp.complete();
        });
    }   
    
    if (url.endsWith('export/costbreakdown/hourly') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: exportCostBreakdownHourly
            }));
            resp.complete();
        });
    }   

}