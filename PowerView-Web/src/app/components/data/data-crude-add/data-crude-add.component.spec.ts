import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatLegacyInputModule as MatInputModule } from '@angular/material/legacy-input';
import { MatLegacySelectModule as MatSelectModule } from '@angular/material/legacy-select';
import { MatLegacySnackBarModule as MatSnackBarModule, MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { LabelsService } from '../../../services/labels.service';
import { CrudeDataService } from 'src/app/services/crude-data.service';
import { ObisTranslateService } from 'src/app/services/obis-translate.service';
import { TranslateService } from '@ngx-translate/core';

import { mock, instance, when, verify, anyString } from 'ts-mockito';

import { DataCrudeTableComponent } from '../data-crude-table/data-crude-table.component';

import { DataCrudeAddComponent } from './data-crude-add.component';

describe('DataCrudeAddComponent', () => {
  let component: DataCrudeAddComponent;
  let fixture: ComponentFixture<DataCrudeAddComponent>;

  let log = mock(NGXLogger);
  let labelsService = mock(LabelsService);
  let crudeDataService = mock(CrudeDataService);
  let obisService = mock(ObisTranslateService);
  let snackBar = mock(MatSnackBar);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [
        DataCrudeTableComponent,
        DataCrudeAddComponent],
      imports: [
        HttpClientTestingModule,
        TranslateModule.forRoot({
          loader: {
            provide: TranslateLoader,
            useFactory: HttpLoaderFactory,
            deps: [HttpClient]
          }
        }),
        RouterTestingModule.withRoutes([]),
        ReactiveFormsModule,
        BrowserAnimationsModule,
        MatInputModule,
        MatTableModule,
        MatSelectModule,
        MatDatepickerModule,
        MatMomentDateModule,
        MatSnackBarModule
      ],
      providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: LabelsService, useValue: instance(labelsService) },
        { provide: CrudeDataService, useValue: instance(crudeDataService)},
        { provide: ObisTranslateService, useValue: instance(obisService) },
        { provide: MatSnackBar, useValue: instance(snackBar) }
      ]
    })
    .compileComponents();
    
    when(labelsService.getLabels()).thenReturn(of([]));

    fixture = TestBed.createComponent(DataCrudeAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
