import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { MatInputModule, MatMenuModule, MatButtonModule, MatTableModule, MatSortModule, MatCheckboxModule, MatSnackBarModule, MatSnackBar } from '@angular/material';
import { ColorPickerModule } from 'ngx-color-picker';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SettingsService } from '../../../services/settings.service';
import { SerieColorSet } from '../../../model/serieColorSet';
import { mock, instance, when, verify } from 'ts-mockito';
import { SettingsSerieColorsTableComponent } from '../settings-serie-colors-table/settings-serie-colors-table.component';

import { SettingsSerieColorsComponent } from './settings-serie-colors.component';

describe('SettingsSerieColorsComponent', () => {
  let component: SettingsSerieColorsComponent;
  let fixture: ComponentFixture<SettingsSerieColorsComponent>;

  let log = mock(NGXLogger);    
  let settingsService = mock(SettingsService);
  let snackBar = mock(MatSnackBar);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        SettingsSerieColorsComponent,
        SettingsSerieColorsTableComponent 
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

    fixture = TestBed.createComponent(SettingsSerieColorsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
