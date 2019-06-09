import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule, MatMenuModule, MatButtonModule, MatTableModule, MatSortModule, MatSnackBarModule, MatSelectModule, MatSnackBar } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SettingsService } from '../services/settings.service';
import { DisconnectRuleSet } from '../model/disconnectRuleSet';
import { DisconnectRuleOptionSet } from '../model/disconnectRuleOptionSet';
import { mock, instance, when, verify } from 'ts-mockito';
import { SettingsRelayControlsTableComponent } from '../settings-relay-controls-table/settings-relay-controls-table.component';

import { SettingsRelayControlsComponent } from './settings-relay-controls.component';

describe('SettingsRelayControlsComponent', () => {
  let component: SettingsRelayControlsComponent;
  let fixture: ComponentFixture<SettingsRelayControlsComponent>;

  let log = mock(NGXLogger);    
  let settingsService = mock(SettingsService);
  let snackBar = mock(MatSnackBar);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        SettingsRelayControlsComponent,
        SettingsRelayControlsTableComponent
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
        BrowserAnimationsModule,
        ReactiveFormsModule,
        MatInputModule,
        MatMenuModule,
        MatButtonModule,
        MatTableModule,
        MatSelectModule,
        MatSortModule,
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
    when(settingsService.getDisconnectRules()).thenReturn(of(new DisconnectRuleSet()));
    when(settingsService.getDisconnectRuleOptions()).thenReturn(of(new DisconnectRuleOptionSet()));

    fixture = TestBed.createComponent(SettingsRelayControlsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
