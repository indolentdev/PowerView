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
import { CrudeDataService } from 'src/app/services/crude-data.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { DataCrudeAddComponent } from './data-crude-add.component';

describe('DataCrudeAddComponent', () => {
  let component: DataCrudeAddComponent;
  let fixture: ComponentFixture<DataCrudeAddComponent>;

  let log = mock(NGXLogger);
  let labelsService = mock(LabelsService);
  let crudeDataService = mock(CrudeDataService);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DataCrudeAddComponent ],
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
        { provide: LabelsService, useValue: instance(labelsService) },
        { provide: CrudeDataService, useValue: instance(CrudeDataService)}
      ]
    })
    .compileComponents();
    
    when(labelsService.getLabels()).thenReturn(of([]));

    fixture = TestBed.createComponent(DataCrudeAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
