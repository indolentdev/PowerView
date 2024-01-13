import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { profileDay } from './profile-day.json';
import { profileMonth } from './profile-month.json';
import { profileYear } from './profile-year.json';
import { profileDecade } from './profile-decade.json';

export function profileBackend(url: string, method: string, request: HttpRequest<any>): Observable<HttpEvent<any>> {

    if (url.indexOf('/profile/day') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    page: profileDay.page,
                    startTime: profileDay.startTime,
                    graphs: profileDay.graphs,
                    periodTotals: profileDay.periodTotals
                }
            }));
            resp.complete();
        });            
    }

    if (url.indexOf('/profile/month') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    page: profileMonth.page,
                    startTime: profileMonth.startTime,
                    graphs: profileMonth.graphs,
                    periodTotals: profileMonth.periodTotals
                }
            }));
            resp.complete();
        });            
    }

    if (url.indexOf('/profile/year') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    page: profileYear.page,
                    startTime: profileYear.startTime,
                    graphs: profileYear.graphs,
                    periodTotals: profileYear.periodTotals
                }
            }));
            resp.complete();
        });            
    }

    if (url.indexOf('/profile/decade') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    page: profileDecade.page,
                    startTime: profileYear.startTime,
                    graphs: profileDecade.graphs,
                    periodTotals: profileDecade.periodTotals
                }
            }));
            resp.complete();
        });
    }

}