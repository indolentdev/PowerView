import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideHttpClientTesting } from "@angular/common/http/testing";
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

import { ExportCostBreakdownHourlyComponent } from './export-cost-breakdown-hourly.component';
import { ExportCostBreakdownComponent } from '../export-cost-breakdown/export-cost-breakdown.component';

describe('ExportCostBreakdownHourlyComponent', () => {
  let component: ExportCostBreakdownHourlyComponent;
  let fixture: ComponentFixture<ExportCostBreakdownHourlyComponent>;

  let log = mock(NGXLogger);
  let exportService = mock(ExportService);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    declarations: [
        ExportCostBreakdownHourlyComponent,
        ExportCostBreakdownComponent
    ],
    imports: [TranslateModule.forRoot({
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
        MatMomentDateModule],
    providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: ExportService, useValue: instance(exportService) },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
})
    .compileComponents();

    when(exportService.getCostBreakdownTitles()).thenReturn(of([])); 

    fixture = TestBed.createComponent(ExportCostBreakdownHourlyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
