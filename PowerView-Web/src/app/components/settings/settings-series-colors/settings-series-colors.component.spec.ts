import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyCheckboxModule as MatCheckboxModule } from '@angular/material/legacy-checkbox';
import { MatLegacyInputModule as MatInputModule } from '@angular/material/legacy-input';
import { MatLegacyMenuModule as MatMenuModule } from '@angular/material/legacy-menu';
import { MatLegacySnackBarModule as MatSnackBarModule, MatLegacySnackBar as MatSnackBar } from '@angular/material/legacy-snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
import { ColorPickerModule } from 'ngx-color-picker';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SettingsService } from '../../../services/settings.service';
import { SerieColorSet } from '../../../model/serieColorSet';
import { mock, instance, when, verify } from 'ts-mockito';
import { SettingsSeriesColorsTableComponent } from '../settings-series-colors-table/settings-series-colors-table.component';

import { SettingsSeriesColorsComponent } from './settings-series-colors.component';

describe('SettingsSerieColorsComponent', () => {
  let component: SettingsSeriesColorsComponent;
  let fixture: ComponentFixture<SettingsSeriesColorsComponent>;

  let log = mock(NGXLogger);    
  let settingsService = mock(SettingsService);
  let snackBar = mock(MatSnackBar);

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        SettingsSeriesColorsComponent,
        SettingsSeriesColorsTableComponent 
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
        MatTableModule,
        ColorPickerModule,
        MatSnackBarModule,
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: SettingsService, useValue: instance(settingsService)},
        {provide: MatSnackBar, useValue: instance(snackBar)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    when(settingsService.getSerieColors()).thenReturn(of(new SerieColorSet()));

    fixture = TestBed.createComponent(SettingsSeriesColorsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
