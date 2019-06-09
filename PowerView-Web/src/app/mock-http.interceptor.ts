import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NGXLogger } from 'ngx-logger';

import { profileBackend } from './mock-backends/profile.backend';
import { diffBackend } from './mock-backends/diff.backend';
import { gaugesBackend } from './mock-backends/gauges.backend';
import { eventsBackend } from './mock-backends/events.backend';
import { settingsBackend } from './mock-backends/settings.backend';

@Injectable({
    providedIn: 'root'
  })
export class MockHttpInterceptor implements HttpInterceptor {

  constructor(private log: NGXLogger) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let url: string = request.url;
    let method: string = request.method;

    this.log.debug(`Request - url ${url} - method ${method}`);

    return profileBackend(url, method, request) ||
      diffBackend(url, method, request) ||
      gaugesBackend(url, method, request) ||
      eventsBackend(url, method, request) ||
      settingsBackend(url, method, request) ||
      next.handle(request); // fallback in case url isn't caught
    }
}
