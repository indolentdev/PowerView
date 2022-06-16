import { HttpRequest, HttpResponse, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';

import { settingsDisconnectrules } from './settings-disconnectrules.json';
import { settingsDisconnectrulesOptions } from './settings-disconnectrules-options.json';
import { settingsEmailRecipients } from './settings-email-recipients.json';
import { settingsApplication } from './settings-application.json';
import { settingsMqtt } from './settings-mqtt.json';
import { settingsProfilePageNames } from './settings-profile-page-names.json.js';
import { settingsProfileGraphs } from './settings-profile-graphs.json';
import { settingsProfileGraphsSeries } from './settings-profile-graphs-series.json';
import { settingsSerieColors } from './settings-serie-colors.json';
import { settingsSmtp } from './settings-smtp.json';

export function settingsBackend(url: string, method: string, request: HttpRequest<any>): Observable<HttpEvent<any>> {

    if (url.indexOf('settings/disconnectrules/options') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    disconnectControlItems: settingsDisconnectrulesOptions.disconnectControlItems,
                    evaluationItems: settingsDisconnectrulesOptions.evaluationItems
                }
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/disconnectrules/names') > -1 && method === "DELETE") {
//        return throwError(new HttpErrorResponse({status:415}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/disconnectrules') > -1 && method === "POST") {
//        return throwError(new HttpErrorResponse({status:415}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }
        
    if (url.indexOf('settings/disconnectrules') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    items: settingsDisconnectrules.items
                }
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/emailrecipients') > -1 && url.indexOf('/test') > -1 && method === "PUT") {
//        return throwError(new HttpErrorResponse({status:567})); // 404, 400, 504, 567
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }
        
    if (url.indexOf('settings/emailrecipients') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    items: settingsEmailRecipients.items
                }
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/emailrecipients') > -1 && method === "POST") {
//        return throwError(new HttpErrorResponse({status:409}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/emailrecipients') > -1 && method === "DELETE") {
//        return throwError(new HttpErrorResponse({status:409}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }
        
    if (url.indexOf('settings/mqtt') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    server: settingsMqtt.server,
                    port: settingsMqtt.port,
                    publishEnabled: settingsMqtt.publishEnabled,
                    clientId: settingsMqtt.clientId
                }
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/application') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    version: settingsApplication.version,
                    culture: settingsApplication.culture,
                    timeZone: settingsApplication.timeZone
                }
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/mqtt') > -1 && method === "PUT") {
//        return throwError(new HttpErrorResponse({status:415}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/profilegraphs/pages') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    items: settingsProfilePageNames.items
                }
            }));
            resp.complete();
        });            
    }

    if (url.indexOf('settings/profilegraphs/series') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    items: settingsProfileGraphsSeries.items
                }
            }));
            resp.complete();
        });            
    }

    if (url.indexOf('settings/profilegraphs/swaprank') > -1 && method === "PUT") {
//        return throwError(new HttpErrorResponse({status:409}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/profilegraphs') > -1 && method === "POST") {
//        return throwError(new HttpErrorResponse({status:415}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/profilegraphs/modify') > -1 && method === "PUT") {
//        return throwError(new HttpErrorResponse({status:415}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }
        
    if (url.indexOf('settings/profilegraphs') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    items: settingsProfileGraphs.items
                }
            }));
            resp.complete();
        });            
    }

    if (url.indexOf('settings/profilegraphs') > -1 && method === "DELETE") {
//        return throwError(new HttpErrorResponse({status:500}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/seriecolors') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    items: settingsSerieColors.items
                }
            }));
            resp.complete();
        });            
    }

    if (url.indexOf('settings/seriecolors') > -1 && method === "PUT") {
//        return throwError(new HttpErrorResponse({status:415}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });            
    }

    if (url.indexOf('settings/smtp') > -1 && method === "GET") {
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 200,
                body: {
                    server: settingsSmtp.server,
                    port: settingsSmtp.port,
                    user: settingsSmtp.user,
                    auth: settingsSmtp.auth,
                    email: settingsSmtp.email
                }
            }));
            resp.complete();
        });
    }

    if (url.indexOf('settings/mqtt') > -1 && method === "PUT") {
//        return throwError(new HttpErrorResponse({status:415}));
        return new Observable(resp => {
            resp.next(new HttpResponse({
                status: 204
            }));
            resp.complete();
        });
    }

}