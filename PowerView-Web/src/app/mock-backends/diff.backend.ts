import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { diff } from './diff.json.js';

export function diffBackend(url: string, method: string, request: HttpRequest<any>): Observable<HttpEvent<any>> {

    if (url.endsWith('diff') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    from: diff.from,
                    to: diff.to,
                    registers: diff.registers
                }
            }));
            resp.complete();
        });            
    }

}