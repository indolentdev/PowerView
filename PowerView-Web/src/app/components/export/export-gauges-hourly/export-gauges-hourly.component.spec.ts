import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';

import { mock, instance, when, verify } from 'ts-mockito';

import { ExportService } from '../../../services/export.service';

import { ExportGaugesHourlyComponent } from './export-gauges-hourly.component';
import { ExportComponent } from '../export/export.component';

describe('ExportGuagesHourlyComponent', () => {
  let component: ExportGaugesHourlyComponent;
  let fixture: ComponentFixture<ExportGaugesHourlyComponent>;

  let log = mock(NGXLogger);
  let exportService = mock(ExportService);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ 
        ExportGaugesHourlyComponent,
        ExportComponent
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
  });

  beforeEach(() => {
    when(exportService.getLabels()).thenReturn(of([])); 

    fixture = TestBed.createComponent(ExportGaugesHourlyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});