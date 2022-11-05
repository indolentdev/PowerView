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
import { ObisService } from '../../../services/obis.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { SettingsProfileGraphsTableComponent } from './settings-profile-graphs-table.component';

describe('SettingsProfileGraphsTableComponent', () => {
  let component: SettingsProfileGraphsTableComponent;
  let fixture: ComponentFixture<SettingsProfileGraphsTableComponent>;

  let log = mock(NGXLogger);    
  let obisService = mock(ObisService);

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SettingsProfileGraphsTableComponent ],
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
        {provide: NGXLogger, useValue: instance(log)},
        {provide: ObisService, useValue: instance(obisService)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SettingsProfileGraphsTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
