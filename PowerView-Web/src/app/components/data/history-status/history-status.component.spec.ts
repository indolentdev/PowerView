import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { HistoryStatusSet } from 'src/app/model/historyStatusSet';
import { HistoryDataService } from 'src/app/services/history-data.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { HistoryStatusTableComponent } from '../history-status-table/history-status-table.component';

import { HistoryStatusComponent } from './history-status.component';

describe('HistoryStatusComponent', () => {
  let component: HistoryStatusComponent;
  let fixture: ComponentFixture<HistoryStatusComponent>;

  let log = mock(NGXLogger);
  let historyDataService = mock(HistoryDataService);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [
        HistoryStatusTableComponent,
        HistoryStatusComponent
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
        FormsModule,
        ReactiveFormsModule,
        BrowserAnimationsModule,
        MatInputModule,
        MatTableModule,
        MatSelectModule,
        MatSnackBarModule,
        MatDatepickerModule,
        MatMomentDateModule
      ],
      providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: HistoryDataService, useValue: instance(historyDataService) }
      ]
    })
    .compileComponents();

    when(historyDataService.getHistoryStatus()).thenReturn(of(new HistoryStatusSet()));

    fixture = TestBed.createComponent(HistoryStatusComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
