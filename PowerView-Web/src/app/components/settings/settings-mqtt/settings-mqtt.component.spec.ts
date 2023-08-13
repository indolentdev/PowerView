import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { ObisTranslateService } from '../../../services/obis-translate.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { SettingsMqttComponent } from './settings-mqtt.component';

describe('SettingsMqttComponent', () => {
  let component: SettingsMqttComponent;
  let fixture: ComponentFixture<SettingsMqttComponent>;

  let log = mock(NGXLogger);    
  let obisService = mock(ObisTranslateService);
  let snackBar = mock(MatSnackBar);

  beforeEach(waitForAsync(() => {
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
        {provide: ObisTranslateService, useValue: instance(obisService)},
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
