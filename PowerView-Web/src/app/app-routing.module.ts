import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DefaultComponent } from './components/default/default.component';
import { ProfileLast24hComponent } from './components/profiles/profile-last24h/profile-last24h.component';
import { ProfileLast31dComponent } from './components/profiles/profile-last31d/profile-last31d.component';
import { ProfileDayComponent } from './components/profiles/profile-day/profile-day.component';
import { ProfileMonthComponent } from './components/profiles/profile-month/profile-month.component';
import { ProfileLast12mComponent } from './components/profiles/profile-last12m/profile-last12m.component';
import { ProfileYearComponent } from './components/profiles/profile-year/profile-year.component';
import { DiffBydatesComponent } from './components/diff/diff-bydates/diff-bydates.component';
import { GaugesLatestComponent } from './components/gauges/gauges-latest/gauges-latest.component';
import { GaugesBydateComponent } from './components/gauges/gauges-bydate/gauges-bydate.component';
import { EventsLatestComponent } from './components/events/events-latest/events-latest.component';
import { ExportHourlyComponent } from './components/export/export-hourly/export-hourly.component';
import { ExportDiffsHourlyComponent } from './components/export/export-diffs-hourly/export-diffs-hourly.component';
import { ExportGaugesHourlyComponent } from './components/export/export-gauges-hourly/export-gauges-hourly.component';
import { SettingsSeriesColorsComponent } from './components/settings/settings-series-colors/settings-series-colors.component';
import { SettingsProfileGraphsComponent } from './components/settings/settings-profile-graphs/settings-profile-graphs.component';
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
  { path: 'diff/bydates', component: DiffBydatesComponent },
  { path: 'gauges/latest', component: GaugesLatestComponent },
  { path: 'gauges/bydate', component: GaugesBydateComponent },
  { path: 'events/latest', component: EventsLatestComponent },
  { path: 'export/diffs/hourly', component: ExportDiffsHourlyComponent },
  { path: 'export/gauges/hourly', component: ExportGaugesHourlyComponent },
  { path: 'export/hourly', component: ExportHourlyComponent },
  { path: 'settings/seriescolors', component: SettingsSeriesColorsComponent },
  { path: 'settings/profilegraphs', component: SettingsProfileGraphsComponent },
  { path: 'settings/relaycontrols', component: SettingsRelayControlsComponent },
  { path: 'settings/email/recipients', component: SettingsRecipientsComponent },
  { path: 'settings/email/smtp', component: SettingsSmtpComponent },
  { path: 'settings/mqtt', component: SettingsMqttComponent },
  { path: 'help/seriesdescriptions', component: HelpSeriesDescriptionsComponent },
  { path: '**', component: DefaultComponent },
];

@NgModule({
  imports: [ RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' }) ],  
  exports: [ RouterModule ]
})
export class AppRoutingModule { }
