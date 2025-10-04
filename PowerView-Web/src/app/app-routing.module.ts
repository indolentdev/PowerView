import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DefaultComponent } from './components/default/default.component';
import { ProfileLast24hComponent } from './components/profiles/profile-last24h/profile-last24h.component';
import { ProfileLast31dComponent } from './components/profiles/profile-last31d/profile-last31d.component';
import { ProfileDayComponent } from './components/profiles/profile-day/profile-day.component';
import { ProfileMonthComponent } from './components/profiles/profile-month/profile-month.component';
import { ProfileLast12mComponent } from './components/profiles/profile-last12m/profile-last12m.component';
import { ProfileLast10yComponent } from './components/profiles/profile-last10y/profile-last10y.component';
import { ProfileYearComponent } from './components/profiles/profile-year/profile-year.component';
import { ProfileDecadeComponent } from './components/profiles/profile-decade/profile-decade.component';
import { DiffBydatesComponent } from './components/diff/diff-bydates/diff-bydates.component';
import { GaugesLatestComponent } from './components/gauges/gauges-latest/gauges-latest.component';
import { GaugesBydateComponent } from './components/gauges/gauges-bydate/gauges-bydate.component';
import { EventsLatestComponent } from './components/events/events-latest/events-latest.component';
import { DataCrudeAddComponent } from './components/data/data-crude-add/data-crude-add.component';
import { DataCrudeByDateComponent } from './components/data/data-crude-bydate/data-crude-bydate.component';
import { ExportDiffsQuarterlyComponent } from './components/export/export-diffs-quarterly/export-diffs-quarterly.component';
import { ExportDiffsHourlyComponent } from './components/export/export-diffs-hourly/export-diffs-hourly.component';
import { ExportGaugesQuarterlyComponent } from './components/export/export-gauges-quarterly/export-gauges-quarterly.component';
import { ExportGaugesHourlyComponent } from './components/export/export-gauges-hourly/export-gauges-hourly.component';
import { ExportCostBreakdownQuarterlyComponent } from './components/export/export-cost-breakdown-quarterly/export-cost-breakdown-quarterly.component';
import { ExportCostBreakdownHourlyComponent } from './components/export/export-cost-breakdown-hourly/export-cost-breakdown-hourly.component';
import { HistoryStatusComponent } from './components/data/history-status/history-status.component';
import { SettingsSeriesColorsComponent } from './components/settings/settings-series-colors/settings-series-colors.component';
import { SettingsProfileGraphsComponent } from './components/settings/settings-profile-graphs/settings-profile-graphs.component';
import { SettingsCostBreakdownComponent } from './components/settings/pricing/settings-cost-breakdown/settings-cost-breakdown.component';
import { SettingsImportComponent } from './components/settings/pricing/settings-import/settings-import.component';
import { SettingsSeriesComponent } from './components/settings/pricing/settings-series/settings-series.component';
import { SettingsRecipientsComponent } from './components/settings/settings-recipients/settings-recipients.component';
import { SettingsRelayControlsComponent } from './components/settings/settings-relay-controls/settings-relay-controls.component';
import { SettingsSmtpComponent } from './components/settings/settings-smtp/settings-smtp.component';
import { SettingsMqttComponent } from './components/settings/settings-mqtt/settings-mqtt.component';
import { HelpSeriesDescriptionsComponent } from './components/help/help-series-descriptions/help-series-descriptions.component';

const routes: Routes = [
  { path: 'profile/last24h', component: ProfileLast24hComponent },
  { path: 'profile/day', component: ProfileDayComponent },
  { path: 'profile/last31d', component: ProfileLast31dComponent },
  { path: 'profile/month', component: ProfileMonthComponent },
  { path: 'profile/last12m', component: ProfileLast12mComponent },
  { path: 'profile/year', component: ProfileYearComponent },
  { path: 'profile/last10y', component: ProfileLast10yComponent },
  { path: 'profile/decade', component: ProfileDecadeComponent },
  { path: 'diff/bydates', component: DiffBydatesComponent },
  { path: 'gauges/latest', component: GaugesLatestComponent },
  { path: 'gauges/bydate', component: GaugesBydateComponent },
  { path: 'events/latest', component: EventsLatestComponent },
  { path: 'data/crude/add', component: DataCrudeAddComponent },
  { path: 'data/crude/bydate', component: DataCrudeByDateComponent },
  { path: 'data/history/status', component: HistoryStatusComponent },
  { path: 'export/diffs/quarterly', component: ExportDiffsQuarterlyComponent },
  { path: 'export/diffs/hourly', component: ExportDiffsHourlyComponent },
  { path: 'export/gauges/quarterly', component: ExportGaugesQuarterlyComponent },
  { path: 'export/gauges/hourly', component: ExportGaugesHourlyComponent },
  { path: 'export/costbreakdowns/quarterly', component: ExportCostBreakdownQuarterlyComponent },
  { path: 'export/costbreakdowns/hourly', component: ExportCostBreakdownHourlyComponent },
  { path: 'settings/seriescolors', component: SettingsSeriesColorsComponent },
  { path: 'settings/profilegraphs', component: SettingsProfileGraphsComponent },
  { path: 'settings/pricing/costbreakdown', component: SettingsCostBreakdownComponent },
  { path: 'settings/pricing/import', component: SettingsImportComponent },
  { path: 'settings/pricing/series', component: SettingsSeriesComponent },
  { path: 'settings/relaycontrols', component: SettingsRelayControlsComponent },
  { path: 'settings/email/recipients', component: SettingsRecipientsComponent },
  { path: 'settings/email/smtp', component: SettingsSmtpComponent },
  { path: 'settings/mqtt', component: SettingsMqttComponent },
  { path: 'help/seriesdescriptions', component: HelpSeriesDescriptionsComponent },
  { path: '**', component: DefaultComponent },
];

@NgModule({
  imports: [ RouterModule.forRoot(routes, {}) ],  
  exports: [ RouterModule ]
})
export class AppRoutingModule { }
