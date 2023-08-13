import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { GaugesService } from '../../../services/gauges.service';
import { GaugeValueGroupSet } from '../../../model/gaugeValueGroupSet';
import { GaugesLatestComponent } from './gauges-latest.component';
import { GaugesTableComponent } from '../gauges-table/gauges-table.component';

import { mock, instance, when, verify } from 'ts-mockito';

describe('GaugesLatestComponent', () => {
  let component: GaugesLatestComponent;
  let fixture: ComponentFixture<GaugesLatestComponent>;

  let log = mock(NGXLogger);    
  let gaugesService = mock(GaugesService);

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        GaugesLatestComponent,
        GaugesTableComponent 
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
        MatTableModule
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

    fixture = TestBed.createComponent(GaugesLatestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
