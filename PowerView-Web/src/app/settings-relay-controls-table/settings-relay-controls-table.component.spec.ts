import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSortModule, MatTableModule } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SerieService } from '../services/serie.service';

import { mock, instance, when, verify } from 'ts-mockito';
import { SettingsRelayControlsTableComponent } from './settings-relay-controls-table.component';

describe('SettingsRelayControlsTableComponent', () => {
  let component: SettingsRelayControlsTableComponent;
  let fixture: ComponentFixture<SettingsRelayControlsTableComponent>;

  let log = mock(NGXLogger);    
  let serieService = mock(SerieService);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SettingsRelayControlsTableComponent ],
      imports: [
        HttpClientTestingModule,
        TranslateModule.forRoot({
          loader: {
            provide: TranslateLoader,
            useFactory: HttpLoaderFactory,
            deps: [HttpClient]
          }
        }),
        BrowserAnimationsModule,
        MatSortModule,
        MatTableModule
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: SerieService, useValue: instance(serieService)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SettingsRelayControlsTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
