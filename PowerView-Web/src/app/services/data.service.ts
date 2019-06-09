import { isUndefined } from "util";
import { HttpClient, HttpHeaders, HttpParams, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, throwError, EMPTY } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { NGXLogger } from 'ngx-logger';

import { environment } from '../../environments/environment';

const httpHeaders = new HttpHeaders({ 'Accept': 'application/json' });
const httpHeadersWithContent = new HttpHeaders({ 'Content-Type': 'application/json; charset=utf-8', 'Accept': 'application/json' });

@Injectable({
  providedIn: 'root'
})
export class DataService {  
  constructor(private log: NGXLogger, private http: HttpClient) { 
  }

  public get<T>(urlPath: string, params?: HttpParams, errorResult?: T): Observable<T> {
    return this.http.get<T>(this.getUrl(urlPath), this.getHttpOptions(httpHeaders, params) )
    .pipe(
      catchError(this.logErrorAndFallback<T>('get', urlPath, errorResult))
    );
  }

  public put(urlPath: string, content?: any, params?: HttpParams): Observable<any> {
    return this.http.put(this.getUrl(urlPath), content, this.getHttpOptions(httpHeadersWithContent, params) )
    .pipe(catchError(error => {
      this.logError("put", urlPath, error);
      return throwError(error);
    }));
  }

  public post(urlPath: string, content?: any, params?: HttpParams): Observable<any> {
    return this.http.post(this.getUrl(urlPath), content, this.getHttpOptions(httpHeadersWithContent, params) )
    .pipe(catchError(error => {
      this.logError("post", urlPath, error);
      return throwError(error);
    }));
  }

  public delete(urlPath: string, params?: HttpParams): Observable<any> {
    return this.http.delete(this.getUrl(urlPath), this.getHttpOptions(httpHeaders, params) )
    .pipe(catchError(error => {
      this.logError("delete", urlPath, error);
      return throwError(error);
    }));
  }

  private getUrl(urlPart: string) {
    return environment.apiUrl + urlPart;
  }

 
  private getHttpOptions(headers: HttpHeaders, params?: HttpParams) {
    return { headers, params };
  }

  private logErrorAndFallback<T>(method: string, urlPath: string, errorResult?: T) {
    return (error: any): Observable<T> => {
      this.logError(method, urlPath, error);

      if (isUndefined(errorResult))
      {
        return EMPTY;
      }
      else
      {
        return of(errorResult as T);
      }
    };
  }

  private logError(method: string, urlPath: string, error: any): void {
    this.log.info(`${method} for path ${urlPath} failed`, error);
  }

}
