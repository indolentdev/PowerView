import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { GaugesService } from '../../../services/gauges.service';
import { GaugeValueGroupSet } from '../../../model/gaugeValueGroupSet';
import { GaugesTableComponent } from '../gauges-table/gauges-table.component';

import { mock, instance, when, verify } from 'ts-mockito';
import { GaugesBydateComponent } from './gauges-bydate.component';

describe('GaugesBydateComponent', () => {
  let component: GaugesBydateComponent;
  let fixture: ComponentFixture<GaugesBydateComponent>;

  let log = mock(NGXLogger);
  let gaugesService = mock(GaugesService);

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    declarations: [
        GaugesBydateComponent,
        GaugesTableComponent,
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
        MatDatepickerModule,
        MatMomentDateModule],
    providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: GaugesService, useValue: instance(gaugesService) },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
})
    .compileComponents();
  }));

  beforeEach(() => {
    when(gaugesService.getLatestGaugeValues()).thenReturn(of(new GaugeValueGroupSet()));

    fixture = TestBed.createComponent(GaugesBydateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
