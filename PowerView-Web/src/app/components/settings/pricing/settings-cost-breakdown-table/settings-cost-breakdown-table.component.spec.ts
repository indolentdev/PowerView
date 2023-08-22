import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';

import { mock, instance, when, verify } from 'ts-mockito';

import { SettingsCostBreakdownTableComponent } from './settings-cost-breakdown-table.component';

describe('SettingsCostBreakdownTableComponent', () => {
  let component: SettingsCostBreakdownTableComponent;
  let fixture: ComponentFixture<SettingsCostBreakdownTableComponent>;

  let log = mock(NGXLogger);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SettingsCostBreakdownTableComponent ],
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

    fixture = TestBed.createComponent(SettingsCostBreakdownTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
