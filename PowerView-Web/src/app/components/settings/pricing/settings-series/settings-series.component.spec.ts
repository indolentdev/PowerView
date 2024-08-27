import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SettingsService } from '../../../../services/settings.service';
import { GeneratorSeriesSet } from 'src/app/model/generatorSeriesSet';
import { GeneratorBaseSeriesSet } from 'src/app/model/generatorBaseSeriesSet';
import { CostBreakdownSet } from '../../../../model/costBreakdownSet';

import { mock, instance, when, verify, anyString } from 'ts-mockito';

import { SettingsSeriesTableComponent } from '../settings-series-table/settings-series-table.component';

import { SettingsSeriesComponent } from './settings-series.component';

describe('SettingsSeriesComponent', () => {
  let component: SettingsSeriesComponent;
  let fixture: ComponentFixture<SettingsSeriesComponent>;

  let log = mock(NGXLogger);
  let settingsService = mock(SettingsService);
  let snackBar = mock(MatSnackBar);
  let dialog = mock(MatDialog);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    declarations: [SettingsSeriesComponent, SettingsSeriesTableComponent],
    imports: [TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient]
            }
        }),
        FormsModule,
        ReactiveFormsModule,
        BrowserAnimationsModule,
        ReactiveFormsModule,
        MatInputModule,
        MatMenuModule,
        MatButtonModule,
        MatTableModule,
        MatSelectModule,
        MatSortModule,
        MatSnackBarModule],
    providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: SettingsService, useValue: instance(settingsService) },
        { provide: MatSnackBar, useValue: instance(snackBar) },
        { provide: MatDialog, useValue: instance(dialog) },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
})
    .compileComponents();

    when(settingsService.getGeneratorsSeries()).thenReturn(of(new GeneratorSeriesSet()));
    when(settingsService.getGeneratorsBaseSeries()).thenReturn(of(new GeneratorBaseSeriesSet()));
    when(settingsService.getCostBreakdowns()).thenReturn(of(new CostBreakdownSet()));

    fixture = TestBed.createComponent(SettingsSeriesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
