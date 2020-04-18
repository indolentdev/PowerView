import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatInputModule, MatMenuModule, MatButtonModule, MatTableModule, MatSortModule, MatSnackBarModule, MatSelectModule, MatSnackBar } from '@angular/material';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';

import { mock, instance, when, verify } from 'ts-mockito';

import { ExportHourlyComponent } from './export-hourly.component';
import { ExportService } from '../../../services/export.service';

describe('HourComponent', () => {
  let component: ExportHourlyComponent;
  let fixture: ComponentFixture<ExportHourlyComponent>;

  let log = mock(NGXLogger);
  let exportService = mock(ExportService);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        ExportHourlyComponent 
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
        RouterTestingModule.withRoutes([]),
        ReactiveFormsModule,
        BrowserAnimationsModule,
        MatInputModule,
        MatTableModule,
        MatSelectModule,
        MatDatepickerModule,
        MatMomentDateModule
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: ExportService, useValue: instance(exportService)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    when(exportService.getLabels()).thenReturn(of([])); 

    fixture = TestBed.createComponent(ExportHourlyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
