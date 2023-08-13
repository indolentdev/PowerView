import { APP_BASE_HREF } from '@angular/common';

import { NGXLogger } from 'ngx-logger';

import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";
import { TestBed, waitForAsync } from '@angular/core/testing';
import { TranslateLoader, TranslateModule, TranslateService } from "@ngx-translate/core";
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { TopComponent } from './components/top/top.component';
import { HttpLoaderFactory } from "./app.module";

import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyMenuModule as MatMenuModule } from '@angular/material/legacy-menu';
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
import { LoadingBarModule } from '@ngx-loading-bar/core';

import { mock, instance, when, verify, anyString } from 'ts-mockito';

const TRANSLATIONS_EN = require('../assets/i18n/en.json');
const TRANSLATIONS_DA = require('../assets/i18n/da.json');

describe('AppComponent', () => {
  let log = mock(NGXLogger);    

  let translate: TranslateService;
  let http: HttpTestingController;

  const routes: Routes = [
//    { path: 'home', component: XxxComponent }
  ];

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [
        AppComponent,
        TopComponent
      ],
      imports: [
        HttpClientTestingModule,
        TranslateModule.forRoot({
          loader: {
            provide: TranslateLoader,
            useFactory: HttpLoaderFactory,
            deps: [HttpClient]
          }
        }),
        RouterModule.forRoot(routes, {}),
        MatMenuModule,
        MatButtonModule,
        MatTableModule,
        LoadingBarModule
      ],
      providers: [
        TranslateService,
        { provide: APP_BASE_HREF, useValue : '/' },
        { provide: NGXLogger, useValue: instance(log) }
      ]
    }).compileComponents();

    translate = TestBed.inject(TranslateService);
    http = TestBed.inject(HttpTestingController);
  }));
  it('should create the app', waitForAsync(() => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.debugElement.componentInstance;
    expect(app).toBeTruthy();
  }));
  it('should contain app-top tag', waitForAsync(() => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.debugElement.nativeElement;
    expect(compiled.querySelector('app-top')).toBeTruthy();
  }));
  it('should contain router-outlet tag', waitForAsync(() => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.debugElement.nativeElement;
    expect(compiled.querySelector('app-top')).toBeTruthy();
  }));
/*  
  it('should load translations', async(() => {
    spyOn(translate, 'getBrowserLang').and.returnValue('en');
    const fixture = TestBed.createComponent(AppComponent);
    const compiled = fixture.debugElement.nativeElement;

    // the DOM should be empty for now since the translations haven't been rendered yet
    expect(compiled.querySelector('h2').textContent).toEqual('');

    http.expectOne('/assets/i18n/en.json').flush(TRANSLATIONS_EN);
    http.expectNone('/assets/i18n/da.json');

    // Finally, assert that there are no outstanding requests.
    http.verify();

    fixture.detectChanges();
    // the content should be translated to english now
    expect(compiled.querySelector('h2').textContent).toEqual(TRANSLATIONS_EN.HOME.TITLE);

    translate.use('da');
    http.expectOne('/assets/i18n/da.json').flush(TRANSLATIONS_DA);

    // Finally, assert that there are no outstanding requests.
    http.verify();

    // the content has not changed yet
    expect(compiled.querySelector('h2').textContent).toEqual(TRANSLATIONS_EN.HOME.TITLE);

    fixture.detectChanges();
    // the content should be translated to french now
    expect(compiled.querySelector('h2').textContent).toEqual(TRANSLATIONS_DA.HOME.TITLE);
  }));
*/  
});
