import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { ObisService } from '../../../services/obis.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { DataCrudeTableComponent } from './data-crude-table.component';

describe('DataCrudeTableComponent', () => {
  let component: DataCrudeTableComponent;
  let fixture: ComponentFixture<DataCrudeTableComponent>;

  let log = mock(NGXLogger);
  let obisService = mock(ObisService);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DataCrudeTableComponent],
      imports: [
        HttpClientTestingModule,
        TranslateModule.forRoot({
          loader: {
            provide: TranslateLoader,
            useFactory: HttpLoaderFactory,
            deps: [HttpClient]
          }
        }),
        BrowserAnimationsModule,
        MatTableModule
      ],
      providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: ObisService, useValue: instance(obisService) }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DataCrudeTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
