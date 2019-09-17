import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DefaultComponent } from './default/default.component';
import { ProfileLast24hComponent } from './profile-last24h/profile-last24h.component';
import { ProfileDayComponent } from './profile-day/profile-day.component';
import { ProfileMonthComponent } from './profile-month/profile-month.component';
import { ProfileYearComponent } from './profile-year/profile-year.component';
import { DiffBydatesComponent } from './diff-bydates/diff-bydates.component';
import { GaugesLatestComponent } from './gauges-latest/gauges-latest.component';
import { GaugesBydateComponent } from './gauges-bydate/gauges-bydate.component';
import { EventsLatestComponent } from './events-latest/events-latest.component';
import { SettingsSerieColorsComponent } from './settings-serie-colors/settings-serie-colors.component';
import { SettingsProfileGraphsComponent } from './settings-profile-graphs/settings-profile-graphs.component';
import { SettingsRecipientsComponent } from './settings-recipients/settings-recipients.component';
import { SettingsRelayControlsComponent } from './settings-relay-controls/settings-relay-controls.component';
import { SettingsSmtpComponent } from './settings-smtp/settings-smtp.component';
import { SettingsMqttComponent } from './settings-mqtt/settings-mqtt.component';

const routes: Routes = [
  { path: 'profile/last24h', component: ProfileLast24hComponent },
  { path: 'profile/day', component: ProfileDayComponent },
  { path: 'profile/month', component: ProfileMonthComponent },
  { path: 'profile/year', component: ProfileYearComponent },
  { path: 'diff/bydates', component: DiffBydatesComponent },
  { path: 'gauges/latest', component: GaugesLatestComponent },
  { path: 'gauges/bydate', component: GaugesBydateComponent },
  { path: 'events/latest', component: EventsLatestComponent },
  { path: 'settings/seriecolors', component: SettingsSerieColorsComponent },
  { path: 'settings/profilegraphs', component: SettingsProfileGraphsComponent },
  { path: 'settings/relaycontrols', component: SettingsRelayControlsComponent },
  { path: 'settings/email/recipients', component: SettingsRecipientsComponent },
  { path: 'settings/email/smtp', component: SettingsSmtpComponent },
  { path: 'settings/mqtt', component: SettingsMqttComponent },
  { path: '**', component: DefaultComponent },
];

@NgModule({
  imports: [ RouterModule.forRoot(routes) ],  
  exports: [ RouterModule ]
})
export class AppRoutingModule { }
