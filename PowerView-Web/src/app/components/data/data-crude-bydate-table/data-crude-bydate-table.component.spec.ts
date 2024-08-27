import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
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
        ConfirmComponent
    ],
    imports: [TranslateModule.forRoot({
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
        MatMomentDateModule],
    providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: CrudeDataService, useValue: instance(crudeDataService) },
        { provide: MatSnackBar, useValue: instance(snackBar) },
        { provide: MatDialog, useValue: instance(dialog) },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
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
