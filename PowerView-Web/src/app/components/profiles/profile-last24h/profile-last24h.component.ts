import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import moment from 'moment';

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
    var start = now.startOf('minute');
    start = start.set('minute', Math.floor((start.minute() / 10))*10); // truncate down to nearest 10 mins.
    start = start.subtract(1, "days");
    this.maxStartTime = start;
    this.defaultStartTime = start;
  }

  ngOnInit() {
  }
}
