import { HttpRequest, HttpResponse, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

import { profileDay } from './profile-day.json';
import { profileMonth } from './profile-month.json';
import { profileYear } from './profile-year.json';

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
/*
    if (url.endsWith('gauges/custom') && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    timestamp: gaugesCustom.timestamp,
                    groups: gaugesCustom.groups
                }
            }));
            resp.complete();
        });            
    }
*/
/*
        // would normally get the username from a cookie or token
        let username = "hank";

        // find if any user matches login credentials
        let filteredUser = users.filter(user => {
            return user.username === username;
        });

        // check to see if the user exists
        if (filteredUser.length) {
            let user = filteredUser[0];
            return new Observable(resp => {
                resp.next(new HttpResponse({
                    status: 200,
                    body: {
                        "id": user.id,
                        "firstName": user.firstName,
                        "lastName": user.lastName,
                        "username": user.username,
                        "preferredEmail": user.preferredEmail
                    }
                }));
                resp.complete();
            });            
        } else {
            // else return 400 bad request
            return new Observable(resp => {
                resp.next(new HttpResponse({
                    status: 400,
                    body: { error: "Unauthorized" }
                }));
                resp.complete();
            });
        }
*/            
//    }
}