import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { events } from './events.json.js';

export function eventsBackend(url: string, method: string, request: HttpRequest<any>): Observable<HttpEvent<any>> {

    if (url.endsWith('events') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    totalCount: events.totalCount,
                    items: events.items
                }
            }));
            resp.complete();
        });            
    }

}