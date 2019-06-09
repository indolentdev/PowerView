import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSortModule, MatTableModule } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SerieService } from '../services/serie.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { ProfileTotalTableComponent } from './profile-total-table.component';

describe('ProfileTotalTableComponent', () => {
  let component: ProfileTotalTableComponent;
  let fixture: ComponentFixture<ProfileTotalTableComponent>;

  let log = mock(NGXLogger);    
  let serieService = mock(SerieService);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ProfileTotalTableComponent ],
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
        MatTableModule,
        MatSortModule
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: SerieService, useValue: instance(serieService)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProfileTotalTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
