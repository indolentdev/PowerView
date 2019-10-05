import { BrowserModule } from '@angular/platform-browser';
import { LOCALE_ID, NgModule } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { TranslateModule, TranslateLoader, TranslateService } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import localeDa from '@angular/common/locales/da';
registerLocaleData(localeDa);
import localeEnGB from '@angular/common/locales/en-GB';
registerLocaleData(localeEnGB);

import { LoggerModule, NgxLoggerLevel } from 'ngx-logger';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatInputModule, MatMenuModule, MatButtonModule, MatTableModule, MatSortModule, MatCheckboxModule, MatSnackBarModule, MatSelectModule } from '@angular/material';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

import { HighchartsChartModule } from 'highcharts-angular';
import { ColorPickerModule } from 'ngx-color-picker';
import { LoadingBarHttpClientModule } from '@ngx-loading-bar/http-client';

import { MockHttpInterceptor } from './mock-http.interceptor';

import { environment } from '../environments/environment';

import { GaugesService } from './services/gauges.service';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { TopComponent } from './top/top.component';
import { GaugesLatestComponent } from './gauges-latest/gauges-latest.component';
import { GaugesTableComponent } from './gauges-table/gauges-table.component';
import { GaugesBydateComponent } from './gauges-bydate/gauges-bydate.component';
import { DiffBydatesComponent } from './diff-bydates/diff-bydates.component';
import { DiffTableComponent } from './diff-table/diff-table.component';
import { ProfileDayComponent } from './profile-day/profile-day.component';
import { ProfileGraphComponent } from './profile-graph/profile-graph.component';
import { ProfileTotalTableComponent } from './profile-total-table/profile-total-table.component';
import { ProfileComponent } from './profile/profile.component';
import { ProfileMonthComponent } from './profile-month/profile-month.component';
import { ProfileYearComponent } from './profile-year/profile-year.component';
import { EventsLatestComponent } from './events-latest/events-latest.component';
import { EventsTableComponent } from './events-table/events-table.component';
import { SettingsSerieColorsComponent } from './settings-serie-colors/settings-serie-colors.component';
import { SettingsSerieColorsTableComponent } from './settings-serie-colors-table/settings-serie-colors-table.component';
import { SettingsMqttComponent } from './settings-mqtt/settings-mqtt.component';
import { SettingsRelayControlsComponent } from './settings-relay-controls/settings-relay-controls.component';
import { SettingsRelayControlsTableComponent } from './settings-relay-controls-table/settings-relay-controls-table.component';
import { DefaultComponent } from './default/default.component';
import { SettingsProfileGraphsComponent } from './settings-profile-graphs/settings-profile-graphs.component';
import { SettingsProfileGraphsTableComponent } from './settings-profile-graphs-table/settings-profile-graphs-table.component';
import { SettingsSmtpComponent } from './settings-smtp/settings-smtp.component';
import { SettingsRecipientsComponent } from './settings-recipients/settings-recipients.component';
import { SettingsRecipientsTableComponent } from './settings-recipients-table/settings-recipients-table.component';
import { ProfileLast24hComponent } from './profile-last24h/profile-last24h.component';
import { ProfileLast12mComponent } from './profile-last12m/profile-last12m.component';
import { ProfileLast31dComponent } from './profile-last31d/profile-last31d.component';

// AoT requires an exported function for factories
export function HttpLoaderFactory(httpClient: HttpClient) {
  return new TranslateHttpLoader(httpClient);
}

// providers to variable so we can append to it dynamically
let providers: any[] = [
  GaugesService,
  {
    provide: LOCALE_ID,
    useFactory: (translate: TranslateService) => {
      return translate.currentLang;
    },
    deps: [TranslateService]
  }
];

// use mock backend if env variable is set
if (environment.useMockBackend == true) {
  providers.push({
      provide: HTTP_INTERCEPTORS,
      useClass: MockHttpInterceptor,
      multi: true
  });
}

@NgModule({
  declarations: [
    AppComponent,
    TopComponent,
    GaugesLatestComponent,
    GaugesTableComponent,
    GaugesBydateComponent,
    DiffBydatesComponent,
    DiffTableComponent,
    ProfileDayComponent,
    ProfileGraphComponent,
    ProfileTotalTableComponent,
    ProfileComponent,
    ProfileMonthComponent,
    ProfileYearComponent,
    EventsLatestComponent,
    EventsTableComponent,
    SettingsSerieColorsComponent,
    SettingsSerieColorsTableComponent,
    SettingsMqttComponent,
    SettingsRelayControlsComponent,
    SettingsRelayControlsTableComponent,
    DefaultComponent,
    SettingsProfileGraphsComponent,
    SettingsProfileGraphsTableComponent,
    SettingsSmtpComponent,
    SettingsRecipientsComponent,
    SettingsRecipientsTableComponent,
    ProfileLast24hComponent,
    ProfileLast12mComponent,
    ProfileLast31dComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    }),
    LoggerModule.forRoot({ level: NgxLoggerLevel.DEBUG }),
    AppRoutingModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatMenuModule,
    MatButtonModule,
    MatTableModule,
    MatSortModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatSelectModule,
    MatDatepickerModule, 
    MatMomentDateModule,
    HighchartsChartModule,
    ColorPickerModule,
    LoadingBarHttpClientModule
  ],
  providers: providers,
  bootstrap: [AppComponent]
})
export class AppModule { }
