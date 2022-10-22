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
import { LabelsService } from '../../../services/labels.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { DataCrudeBydateTableComponent } from '../data-crude-bydate-table/data-crude-bydate-table.component';
import { DataCrudeTableComponent } from '../data-crude-table/data-crude-table.component';

import { DataCrudeByDateComponent } from './data-crude-bydate.component';

describe('DataCrudeByDateComponent', () => {
  let component: DataCrudeByDateComponent;
  let fixture: ComponentFixture<DataCrudeByDateComponent>;

  let log = mock(NGXLogger);
  let labelsService = mock(LabelsService);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [
        DataCrudeTableComponent,
        DataCrudeBydateTableComponent,
        DataCrudeByDateComponent],
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
        { provide: NGXLogger, useValue: instance(log) },
        { provide: LabelsService, useValue: instance(labelsService) }
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
