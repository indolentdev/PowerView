import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient } from "@angular/common/http";
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatLegacyTableModule as MatTableModule } from '@angular/material/legacy-table';
import { mock, instance, when, verify } from 'ts-mockito';

import { HelpSeriesDescriptionsComponent } from './help-series-descriptions.component';
import { HelpSeriesDescriptionsTableComponent } from '../help-series-descriptions-table/help-series-descriptions-table.component';
import { SeriesMeasureKindsTableComponent } from '../series-measure-kinds-table/series-measure-kinds-table.component';

describe('HelpSeriesDescriptionsComponent', () => {
  let component: HelpSeriesDescriptionsComponent;
  let fixture: ComponentFixture<HelpSeriesDescriptionsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ 
        HelpSeriesDescriptionsComponent,
        HelpSeriesDescriptionsTableComponent,
        SeriesMeasureKindsTableComponent
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
        MatTableModule
      ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HelpSeriesDescriptionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
