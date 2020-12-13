import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
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
import { DiffService } from '../../../services/diff.service';
import { GaugeValueGroupSet } from '../../../model/gaugeValueGroupSet';
import { GaugesTableComponent } from '../../gauges/gauges-table/gauges-table.component';

import { mock, instance, when, verify } from 'ts-mockito';

import { DiffBydatesComponent } from './diff-bydates.component';
import { DiffTableComponent } from '../diff-table/diff-table.component';

describe('DiffBydatesComponent', () => {
  let component: DiffBydatesComponent;
  let fixture: ComponentFixture<DiffBydatesComponent>;

  let log = mock(NGXLogger);
  let diffService = mock(DiffService);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        DiffBydatesComponent,
        DiffTableComponent
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
        {provide: DiffService, useValue: instance(diffService)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DiffBydatesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
