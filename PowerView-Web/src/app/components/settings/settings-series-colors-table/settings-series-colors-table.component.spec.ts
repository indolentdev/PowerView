import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { ColorPickerModule } from 'ngx-color-picker';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SerieService } from '../../../services/serie.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { SettingsSeriesColorsTableComponent } from './settings-series-colors-table.component';

describe('SettingsSerieColorsTableComponent', () => {
  let component: SettingsSeriesColorsTableComponent;
  let fixture: ComponentFixture<SettingsSeriesColorsTableComponent>;

  let log = mock(NGXLogger);    
  let serieService = mock(SerieService);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SettingsSeriesColorsTableComponent ],
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
        MatTableModule,
        ColorPickerModule
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: SerieService, useValue: instance(serieService)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SettingsSeriesColorsTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
