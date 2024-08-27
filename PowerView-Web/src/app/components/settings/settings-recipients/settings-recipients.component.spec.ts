import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { HttpLoaderFactory } from "../../../app.module";
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { NGXLogger } from 'ngx-logger';
import { of } from 'rxjs';
import { SettingsService } from '../../../services/settings.service';
import { EmailRecipientSet } from '../../../model/emailRecipientSet';
import { mock, instance, when, verify } from 'ts-mockito';
import { SettingsRecipientsTableComponent} from '../settings-recipients-table/settings-recipients-table.component';

import { SettingsRecipientsComponent } from './settings-recipients.component';

describe('SettingsRecipientsComponent', () => {
  let component: SettingsRecipientsComponent;
  let fixture: ComponentFixture<SettingsRecipientsComponent>;

  let log = mock(NGXLogger);    
  let settingsService = mock(SettingsService);
  let snackBar = mock(MatSnackBar);

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    declarations: [
        SettingsRecipientsComponent,
        SettingsRecipientsTableComponent
    ],
    imports: [TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient]
            }
        }),
        BrowserAnimationsModule,
        ReactiveFormsModule,
        MatInputModule,
        MatMenuModule,
        MatButtonModule,
        MatTableModule,
        MatSelectModule,
        MatSortModule,
        MatSnackBarModule],
    providers: [
        { provide: NGXLogger, useValue: instance(log) },
        { provide: SettingsService, useValue: instance(settingsService) },
        { provide: MatSnackBar, useValue: instance(snackBar) },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
})
    .compileComponents();
  }));

  beforeEach(() => {
    when(settingsService.getEmailRecipients()).thenReturn(of(new EmailRecipientSet()));

    fixture = TestBed.createComponent(SettingsRecipientsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
