import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

export function obisBackend(url: string, method: string, request: HttpRequest<any>): Observable<HttpEvent<any>> {

    if (url.endsWith('obis/codes') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: { obisCodes: ["8.0.1.4.0.255", "6.0.1.1.0.255", "8.0.1.1.0.255"] }
            }));
            resp.complete();
        });
    }

}