import { Component, OnInit,ViewChild, Input } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { MatTableModule } from '@angular/material';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { HighchartsChartModule } from 'highcharts-angular';

import { mock, instance, when, verify } from 'ts-mockito';

import { ProfileGraphComponent } from './profile-graph.component';
import { Profile } from '../../../model/profile';

describe('ProfileGraphComponent', () => {
  let testHostComponent: TestHostComponent;
  let testHostFixture: ComponentFixture<TestHostComponent>;  

  let log = mock(NGXLogger);    

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        TestHostComponent, ProfileGraphComponent 
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
        HighchartsChartModule
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    testHostFixture = TestBed.createComponent(TestHostComponent);
    testHostComponent = testHostFixture.componentInstance;
    testHostFixture.detectChanges();
  });

  it('should create', () => {
    testHostComponent.setProfileGraph(new Profile);
    testHostFixture.detectChanges();

    expect(testHostComponent).toBeTruthy();
  });

  @Component({
    selector: `host-component`,
    template: `<app-profile-graph [profileGraph]="profileGraph" [timeFormat]="timeFormat"></app-profile-graph>`
  })
  class TestHostComponent {
    profileGraph: Profile;
    timeFormat: string;

    constructor() {
      this.profileGraph = new Profile;
      this.timeFormat = "LT";
    }

    setProfileGraph(pg: Profile) {
      this.profileGraph = pg;
    }

    setTimeFormat(tf: string) {
      this.timeFormat = tf;
    }
}
});
