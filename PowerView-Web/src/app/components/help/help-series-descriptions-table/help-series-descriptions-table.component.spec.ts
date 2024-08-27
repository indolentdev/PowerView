import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTableModule } from '@angular/material/table';
import { mock, instance, when, verify } from 'ts-mockito';

import { HelpSeriesDescriptionsTableComponent } from './help-series-descriptions-table.component';

describe('HelpSeriesDescriptionsTableComponent', () => {
  let component: HelpSeriesDescriptionsTableComponent;
  let fixture: ComponentFixture<HelpSeriesDescriptionsTableComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    declarations: [HelpSeriesDescriptionsTableComponent],
    imports: [TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient]
            }
        }),
        BrowserAnimationsModule,
        MatTableModule],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(HelpSeriesDescriptionsTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
