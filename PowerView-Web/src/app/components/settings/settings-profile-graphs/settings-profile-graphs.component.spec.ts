import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule, MatMenuModule, MatButtonModule, MatTableModule, MatSortModule, MatSnackBarModule, MatSelectModule, MatSnackBar } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SettingsService } from '../../../services/settings.service';
import { ProfileGraphSet } from '../../../model/profileGraphSet';
import { ProfileGraphSerieSet } from '../../../model/profileGraphSerieSet';
import { mock, instance, when, verify } from 'ts-mockito';
import { SettingsProfileGraphsTableComponent } from '../settings-profile-graphs-table/settings-profile-graphs-table.component';

import { SettingsProfileGraphsComponent } from './settings-profile-graphs.component';

describe('SettingsProfileGraphsComponent', () => {
  let component: SettingsProfileGraphsComponent;
  let fixture: ComponentFixture<SettingsProfileGraphsComponent>;

  let log = mock(NGXLogger);    
  let settingsService = mock(SettingsService);
  let snackBar = mock(MatSnackBar);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        SettingsProfileGraphsComponent,
        SettingsProfileGraphsTableComponent
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
    when(settingsService.getProfileGraphs()).thenReturn(of(new ProfileGraphSet()));
    when(settingsService.getProfileGraphsSeries()).thenReturn(of(new ProfileGraphSerieSet()));

    fixture = TestBed.createComponent(SettingsProfileGraphsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
