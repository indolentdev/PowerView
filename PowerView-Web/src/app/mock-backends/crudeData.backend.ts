import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { crudeData } from './crudeData.json.js'

export function crudeDataBackend(url: string, method: string, request: HttpRequest<any>): Observable<HttpEvent<any>> {

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

}