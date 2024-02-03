import { BrowserModule } from '@angular/platform-browser';
import { LOCALE_ID, NgModule } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { TranslateModule, TranslateLoader, TranslateService } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import localeDa from '@angular/common/locales/da';
registerLocaleData(localeDa);
import localeEnGB from '@angular/common/locales/en-GB';
registerLocaleData(localeEnGB);

import { LoggerModule, NgxLoggerLevel } from 'ngx-logger';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
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
import { TopComponent } from './components/top/top.component';
import { GaugesLatestComponent } from './components/gauges/gauges-latest/gauges-latest.component';
import { GaugesTableComponent } from './components/gauges/gauges-table/gauges-table.component';
import { GaugesBydateComponent } from './components/gauges/gauges-bydate/gauges-bydate.component';
import { DiffBydatesComponent } from './components/diff/diff-bydates/diff-bydates.component';
import { DiffTableComponent } from './components/diff/diff-table/diff-table.component';
import { ProfileDayComponent } from './components/profiles/profile-day/profile-day.component';
import { ProfileGraphComponent } from './components/profiles/profile-graph/profile-graph.component';
import { ProfileTotalTableComponent } from './components/profiles/profile-total-table/profile-total-table.component';
import { ProfileComponent } from './components/profiles/profile/profile.component';
import { ProfileMonthComponent } from './components/profiles/profile-month/profile-month.component';
import { ProfileYearComponent } from './components/profiles/profile-year/profile-year.component';
import { ProfileDecadeComponent } from './components/profiles/profile-decade/profile-decade.component';
import { EventsLatestComponent } from './components/events/events-latest/events-latest.component';
import { EventsTableComponent } from './components/events/events-table/events-table.component';
import { SettingsSeriesColorsComponent } from './components/settings/settings-series-colors/settings-series-colors.component';
import { SettingsSeriesColorsTableComponent } from './components/settings/settings-series-colors-table/settings-series-colors-table.component';
import { SettingsMqttComponent } from './components/settings/settings-mqtt/settings-mqtt.component';
import { SettingsRelayControlsComponent } from './components/settings/settings-relay-controls/settings-relay-controls.component';
import { SettingsRelayControlsTableComponent } from './components/settings/settings-relay-controls-table/settings-relay-controls-table.component';
import { DefaultComponent } from './components/default/default.component';
import { SettingsProfileGraphsComponent } from './components/settings/settings-profile-graphs/settings-profile-graphs.component';
import { SettingsProfileGraphsTableComponent } from './components/settings/settings-profile-graphs-table/settings-profile-graphs-table.component';
import { SettingsSmtpComponent } from './components/settings/settings-smtp/settings-smtp.component';
import { SettingsRecipientsComponent } from './components/settings/settings-recipients/settings-recipients.component';
import { SettingsRecipientsTableComponent } from './components/settings/settings-recipients-table/settings-recipients-table.component';
import { ProfileLast24hComponent } from './components/profiles/profile-last24h/profile-last24h.component';
import { ProfileLast12mComponent } from './components/profiles/profile-last12m/profile-last12m.component';
import { ProfileLast31dComponent } from './components/profiles/profile-last31d/profile-last31d.component';
import { HelpSeriesDescriptionsComponent } from './components/help/help-series-descriptions/help-series-descriptions.component';
import { HelpSeriesDescriptionsTableComponent } from './components/help/help-series-descriptions-table/help-series-descriptions-table.component';
import { SeriesMeasureKindsTableComponent } from './components/help/series-measure-kinds-table/series-measure-kinds-table.component';
import { ExportComponent } from './components/export/export/export.component';
import { ExportGaugesHourlyComponent } from './components/export/export-gauges-hourly/export-gauges-hourly.component';
import { ExportDiffsHourlyComponent } from './components/export/export-diffs-hourly/export-diffs-hourly.component';
import { DataCrudeTableComponent } from './components/data/data-crude-table/data-crude-table.component';
import { DataCrudeByDateComponent } from './components/data/data-crude-bydate/data-crude-bydate.component';
import { ScalerPipe } from './pipes/scaler.pipe';
import { DataCrudeBydateTableComponent } from './components/data/data-crude-bydate-table/data-crude-bydate-table.component';
import { DataCrudeAddComponent } from './components/data/data-crude-add/data-crude-add.component';
import { ConfirmComponent } from './components/confirm/confirm.component';
import { SettingsCostBreakdownComponent } from './components/settings/pricing/settings-cost-breakdown/settings-cost-breakdown.component';
import { SettingsCostBreakdownTableComponent } from './components/settings/pricing/settings-cost-breakdown-table/settings-cost-breakdown-table.component';
import { SettingsCostBreakdownEntryComponent } from './components/settings/pricing/settings-cost-breakdown-entry/settings-cost-breakdown-entry.component';
import { SettingsCostBreakdownEntryTableComponent } from './components/settings/pricing/settings-cost-breakdown-entry-table/settings-cost-breakdown-entry-table.component';
import { ExportCostBreakdownHourlyComponent } from './components/export/export-cost-breakdown-hourly/export-cost-breakdown-hourly.component';
import { ExportCostBreakdownComponent } from './components/export/export-cost-breakdown/export-cost-breakdown.component';
import { SettingsImportComponent } from './components/settings/pricing/settings-import/settings-import.component';
import { SettingsImportTableComponent } from './components/settings/pricing/settings-import-table/settings-import-table.component';
import { SettingsImportEnergiDataServiceComponent } from './components/settings/pricing/settings-import-energi-data-service/settings-import-energi-data-service.component';
import { ProfileLast10yComponent } from './components/profiles/profile-last10y/profile-last10y.component';
import { HistoryStatusComponent } from './components/data/history-status/history-status.component';
import { HistoryStatusTableComponent } from './components/data/history-status-table/history-status-table.component';

// AoT requires an exported function for factories
export function HttpLoaderFactory(httpClient: HttpClient) {
  return new TranslateHttpLoader(httpClient);
}

export class DynamicLocaleId extends String {
  constructor(protected service: TranslateService) {
    super('');
  }

  toString() {
    return this.service.currentLang;
  }
}

// providers to variable so we can append to it dynamically
let providers: any[] = [
  GaugesService,
  { provide: LOCALE_ID, useClass: DynamicLocaleId, deps: [TranslateService] }
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
    ProfileDecadeComponent,
    EventsLatestComponent,
    EventsTableComponent,
    SettingsSeriesColorsComponent,
    SettingsSeriesColorsTableComponent,
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
    ProfileLast31dComponent,
    HelpSeriesDescriptionsComponent,
    HelpSeriesDescriptionsTableComponent,
    SeriesMeasureKindsTableComponent,
    ExportComponent,
    ExportGaugesHourlyComponent,
    ExportDiffsHourlyComponent,
    DataCrudeTableComponent,
    DataCrudeByDateComponent,
    ScalerPipe,
    DataCrudeBydateTableComponent,
    DataCrudeAddComponent,
    ConfirmComponent,
    SettingsCostBreakdownComponent,
    SettingsCostBreakdownTableComponent,
    SettingsCostBreakdownEntryComponent,
    SettingsCostBreakdownEntryTableComponent,
    ExportCostBreakdownHourlyComponent,
    ExportCostBreakdownComponent,
    SettingsImportComponent,
    SettingsImportTableComponent,
    SettingsImportEnergiDataServiceComponent,
    ProfileLast10yComponent,
    HistoryStatusComponent,
    HistoryStatusTableComponent
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
    FormsModule,
    ReactiveFormsModule,
    MatInputModule,
    MatMenuModule,
    MatButtonModule,
    MatTableModule,
    MatSortModule,
    MatCheckboxModule,
    MatDialogModule,
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
