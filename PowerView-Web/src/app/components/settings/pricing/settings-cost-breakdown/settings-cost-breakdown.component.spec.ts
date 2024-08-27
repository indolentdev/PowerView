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
import { CostBreakdownSet } from '../../../../model/costBreakdownSet';

import { mock, instance, when, verify, anyString } from 'ts-mockito';

import { ConfirmComponent } from '../../../confirm/confirm.component';
import { SettingsCostBreakdownTableComponent } from '../settings-cost-breakdown-table/settings-cost-breakdown-table.component';

import { SettingsCostBreakdownComponent } from './settings-cost-breakdown.component';

describe('SettingsCostBreakdownComponent', () => {
  let component: SettingsCostBreakdownComponent;
  let fixture: ComponentFixture<SettingsCostBreakdownComponent>;

  let log = mock(NGXLogger);
  let settingsService = mock(SettingsService);
  let snackBar = mock(MatSnackBar);
  let dialog = mock(MatDialog);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    declarations: [
        ConfirmComponent,
        SettingsCostBreakdownTableComponent,
        SettingsCostBreakdownComponent
    ],
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

    when(settingsService.getCostBreakdowns()).thenReturn(of(new CostBreakdownSet()));

    fixture = TestBed.createComponent(SettingsCostBreakdownComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
