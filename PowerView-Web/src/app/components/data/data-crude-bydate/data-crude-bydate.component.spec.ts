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
import { LabelsService } from '../../../services/labels.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { DataCrudeBydateTableComponent } from '../data-crude-bydate-table/data-crude-bydate-table.component';
import { DataCrudeTableComponent } from '../data-crude-table/data-crude-table.component';
import { ConfirmComponent } from '../../confirm/confirm.component';

import { DataCrudeByDateComponent } from './data-crude-bydate.component';

describe('DataCrudeByDateComponent', () => {
  let component: DataCrudeByDateComponent;
  let fixture: ComponentFixture<DataCrudeByDateComponent>;

  let log = mock(NGXLogger);
  let labelsService = mock(LabelsService);
  let snackBar = mock(MatSnackBar);
  let dialog = mock(MatDialog);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    declarations: [
        DataCrudeTableComponent,
        DataCrudeBydateTableComponent,
        DataCrudeByDateComponent,
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
        { provide: LabelsService, useValue: instance(labelsService) },
        { provide: MatSnackBar, useValue: instance(snackBar) },
        { provide: MatDialog, useValue: instance(dialog) },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
})
    .compileComponents();

    when(labelsService.getLabels()).thenReturn(of([]));

    fixture = TestBed.createComponent(DataCrudeByDateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
