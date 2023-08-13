import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ReactiveFormsModule } from '@angular/forms';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyCheckboxModule as MatCheckboxModule } from '@angular/material/legacy-checkbox';
import { MatLegacyInputModule as MatInputModule } from '@angular/material/legacy-input';
import { MatLegacyMenuModule as MatMenuModule } from '@angular/material/legacy-menu';
import { MatLegacySnackBarModule as MatSnackBarModule, MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { ObisTranslateService } from '../../../services/obis-translate.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { SettingsSmtpComponent } from './settings-smtp.component';

describe('SettingsSmtpComponent', () => {
  let component: SettingsSmtpComponent;
  let fixture: ComponentFixture<SettingsSmtpComponent>;

  let log = mock(NGXLogger);    
  let obisService = mock(ObisTranslateService);
  let snackBar = mock(MatSnackBar);

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SettingsSmtpComponent ],
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
    fixture = TestBed.createComponent(SettingsSmtpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
