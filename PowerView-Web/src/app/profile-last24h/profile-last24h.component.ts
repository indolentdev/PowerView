import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-profile-last24h',
  templateUrl: './profile-last24h.component.html',
  styleUrls: ['./profile-last24h.component.css']
})
export class ProfileLast24hComponent implements OnInit {
  maxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) { 
    var now = moment();
    var start = now.subtract(1, "days");
    this.maxStartTime = start;
    this.defaultStartTime = start;
  }

  ngOnInit() {
  }
}
