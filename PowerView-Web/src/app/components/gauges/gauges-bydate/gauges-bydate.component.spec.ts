import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyInputModule as MatInputModule } from '@angular/material/legacy-input';
import { MatLegacyMenuModule as MatMenuModule } from '@angular/material/legacy-menu';
import { MatSortModule } from '@angular/material/sort';
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
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
        MatDatepickerModule,
        MatMomentDateModule
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: GaugesService, useValue: instance(gaugesService)}
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
