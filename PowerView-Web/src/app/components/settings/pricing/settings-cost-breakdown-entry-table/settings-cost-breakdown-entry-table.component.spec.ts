import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { ObisTranslateService } from '../../../../services/obis-translate.service';

import { mock, instance, when, verify } from 'ts-mockito';

import { SettingsCostBreakdownEntryTableComponent } from './settings-cost-breakdown-entry-table.component';

describe('SettingsCostBreakdownEntryTableComponent', () => {
  let component: SettingsCostBreakdownEntryTableComponent;
  let fixture: ComponentFixture<SettingsCostBreakdownEntryTableComponent>;

  let log = mock(NGXLogger);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    declarations: [SettingsCostBreakdownEntryTableComponent],
    imports: [TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient]
            }
        }),
        BrowserAnimationsModule,
        MatSortModule,
        MatTableModule],
    providers: [
        { provide: NGXLogger, useValue: instance(log) },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
})
      .compileComponents();

    fixture = TestBed.createComponent(SettingsCostBreakdownEntryTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
