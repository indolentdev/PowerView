import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { ObisTranslateService } from '../../../services/obis-translate.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { ProfileTotalTableComponent } from './profile-total-table.component';

describe('ProfileTotalTableComponent', () => {
  let component: ProfileTotalTableComponent;
  let fixture: ComponentFixture<ProfileTotalTableComponent>;

  let log = mock(NGXLogger);    
  let obisService = mock(ObisTranslateService);

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    declarations: [ProfileTotalTableComponent],
    imports: [TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient]
            }
        }),
        BrowserAnimationsModule,
        MatTableModule,
        MatSortModule],
    providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: ObisTranslateService, useValue: instance(obisService) },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
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
