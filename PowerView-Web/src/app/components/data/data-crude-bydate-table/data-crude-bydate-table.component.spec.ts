import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatLegacyDialog as MatDialog } from '@angular/material/legacy-dialog';
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
import { CrudeDataService } from '../../../services/crude-data.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { DataCrudeTableComponent } from '../data-crude-table/data-crude-table.component';
import { ConfirmComponent } from '../../confirm/confirm.component';

import { DataCrudeBydateTableComponent } from './data-crude-bydate-table.component';

describe('DataCrudeBydateTableComponent', () => {
  let component: DataCrudeBydateTableComponent;
  let fixture: ComponentFixture<DataCrudeBydateTableComponent>;

  let log = mock(NGXLogger);
  let crudeDataService = mock(CrudeDataService);
  let snackBar = mock(MatSnackBar);
  let dialog = mock(MatDialog);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [
        DataCrudeTableComponent,
        DataCrudeBydateTableComponent,
        ConfirmComponent],
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
        FormsModule,
        ReactiveFormsModule,
        BrowserAnimationsModule,
        MatInputModule,
        MatTableModule,
        MatSelectModule,
        MatSnackBarModule,
        MatDatepickerModule,
        MatMomentDateModule
      ],
      providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: CrudeDataService, useValue: instance(crudeDataService) },
        { provide: MatSnackBar, useValue: instance(snackBar) },
        { provide: MatDialog, useValue: instance(dialog) }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DataCrudeBydateTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
