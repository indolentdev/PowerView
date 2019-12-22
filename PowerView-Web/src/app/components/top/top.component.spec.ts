import { APP_BASE_HREF } from '@angular/common';
import { EventEmitter } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NGXLogger } from 'ngx-logger';
import { Observable, of } from 'rxjs';
import { MenuService } from '../../services/menu.service';
import { ProfilePageNameSet } from '../../model/profilePageNameSet';

import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../app.module";
import { RouterModule, Routes } from '@angular/router';
import { MatMenuModule, MatButtonModule } from '@angular/material';

import { TopComponent } from './top.component';

import { mock, instance, when, verify, anyString, reset } from 'ts-mockito';

describe('TopComponent', () => {
  let component: TopComponent;
  let fixture: ComponentFixture<TopComponent>;

  let log = mock(NGXLogger);
  let menuService: MenuService;
  let profileGraphPagesChange: EventEmitter<any>;

  const routes: Routes = [
    //    { path: 'home', component: XxxComponent }
  ];
    
  beforeEach(async(() => {
    menuService = mock(MenuService);

    TestBed.configureTestingModule({
      declarations: [ TopComponent ],
      imports: [
        HttpClientTestingModule,
        TranslateModule.forRoot({
          loader: {
            provide: TranslateLoader,
            useFactory: HttpLoaderFactory,
            deps: [HttpClient]
          }
        }),
        RouterModule.forRoot(routes),
        MatMenuModule,       
        MatButtonModule        
      ],
      providers: [
        { provide: APP_BASE_HREF, useValue : '/' },
        { provide: NGXLogger, useValue: instance(log) },
        { provide: MenuService, useValue: instance(menuService) }
      ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    profileGraphPagesChange = new EventEmitter();

    when(menuService.getProfileGraphPageChanges()).thenReturn(profileGraphPagesChange);
    when(menuService.getProfilePageNames(anyString())).thenReturn(of(new ProfilePageNameSet));

    fixture = TestBed.createComponent(TopComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should get day profile pages', () => {
    verify(menuService.getProfilePageNames("day"));
  });

  it('should get month profile pages', () => {
    verify(menuService.getProfilePageNames("month"));
  });

  it('should get year profile pages', () => {
    verify(menuService.getProfilePageNames("year"));
  });

  it('should refresh pages on signal', () => {
    // Arrange

    // Act
    profileGraphPagesChange.emit();

    // Assert
    verify(menuService.getProfilePageNames(anyString())).times(2*3);  // 2 for ngOnInit and emit, 3 for day/month/year
  });
});
