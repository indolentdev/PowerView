import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule, MatMenuModule, MatButtonModule, MatTableModule, MatSortModule, MatCheckboxModule, MatSnackBarModule, MatSnackBar } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SerieService } from '../services/serie.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { SettingsMqttComponent } from './settings-mqtt.component';

describe('SettingsMqttComponent', () => {
  let component: SettingsMqttComponent;
  let fixture: ComponentFixture<SettingsMqttComponent>;

  let log = mock(NGXLogger);    
  let serieService = mock(SerieService);
  let snackBar = mock(MatSnackBar);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SettingsMqttComponent ],
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
        ReactiveFormsModule,
        MatInputModule,
        MatMenuModule,
        MatButtonModule,
        MatTableModule,
        MatSortModule,
        MatCheckboxModule,
        MatSnackBarModule,
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: SerieService, useValue: instance(serieService)},
        {provide: MatSnackBar, useValue: instance(snackBar)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SettingsMqttComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
