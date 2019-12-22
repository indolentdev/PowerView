import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';
import { SettingsService } from '../../services/settings.service';
import { ApplicationProperties } from '../../model/applicationProperties';

@Component({
  selector: 'app-default',
  templateUrl: './default.component.html',
  styleUrls: ['./default.component.css']
})
export class DefaultComponent implements OnInit {
  applicationProperties: ApplicationProperties;

  constructor(private log: NGXLogger, private settingsService: SettingsService) { }

  ngOnInit() {
    this.settingsService.getApplicationProperties().subscribe(applicationProperties => {
      this.applicationProperties = applicationProperties;
    });
  }

}
