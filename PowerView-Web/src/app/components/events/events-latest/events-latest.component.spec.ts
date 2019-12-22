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
import { EventsService } from '../../../services/events.service';
import { EventSet } from '../../../model/eventSet';

import { mock, instance, when, verify } from 'ts-mockito';

import { EventsLatestComponent } from './events-latest.component';
import { EventsTableComponent } from '../events-table/events-table.component';

describe('EventsLatestComponent', () => {
  let component: EventsLatestComponent;
  let fixture: ComponentFixture<EventsLatestComponent>;

  let log = mock(NGXLogger);
  let eventsService = mock(EventsService);

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ 
        EventsLatestComponent,
        EventsTableComponent
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
        MatDatepickerModule,
        MatMomentDateModule
      ],
      providers: [
        {provide: NGXLogger, useValue: instance(log)},
        {provide: EventsService, useValue: instance(eventsService)}
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    when(eventsService.getLatestEvents()).thenReturn(of(new EventSet()));

    fixture = TestBed.createComponent(EventsLatestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
