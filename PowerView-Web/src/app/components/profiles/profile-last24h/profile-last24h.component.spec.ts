import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatInputModule, MatMenuModule, MatButtonModule, MatTableModule, MatSortModule } from '@angular/material';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { ProfileService } from '../../../services/profile.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { ProfileGraphComponent } from '../profile-graph/profile-graph.component';
import { HighchartsChartModule } from 'highcharts-angular';
import { ProfileTotalTableComponent } from '../profile-total-table/profile-total-table.component';
import { ProfileComponent } from '../profile/profile.component';

import { ProfileLast24hComponent } from './profile-last24h.component';

describe('ProfileLatest24hComponent', () => {
  let component: ProfileLast24hComponent;
  let fixture: ComponentFixture<ProfileLast24hComponent>;

  let log = mock(NGXLogger);    
  let profileService = mock(ProfileService);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        ProfileLast24hComponent,
        ProfileTotalTableComponent,
        ProfileGraphComponent,
        ProfileComponent
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
        RouterTestingModule.withRoutes([]),
        ReactiveFormsModule,
        BrowserAnimationsModule,
        MatInputModule,
        MatTableModule,
        MatSortModule,
        MatDatepickerModule,
        MatMomentDateModule,
        HighchartsChartModule
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: ProfileService, useValue: instance(profileService)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProfileLast24hComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});