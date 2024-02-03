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
import { ObisTranslateService } from '../../../services/obis-translate.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { HistoryStatusTableComponent } from './history-status-table.component';

describe('HistoryStatusTableComponent', () => {
  let component: HistoryStatusTableComponent;
  let fixture: ComponentFixture<HistoryStatusTableComponent>;

  let log = mock(NGXLogger);    

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HistoryStatusTableComponent ],
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
        MatSortModule,
        MatTableModule
      ],
      providers: [
        { provide: NGXLogger, useValue: instance(log) }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HistoryStatusTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
