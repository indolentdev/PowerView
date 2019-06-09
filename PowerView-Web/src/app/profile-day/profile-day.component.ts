import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import * as moment from 'moment';

@Component({
  selector: 'app-profile-day',
  templateUrl: './profile-day.component.html',
  styleUrls: ['./profile-day.component.css']
})
export class ProfileDayComponent implements OnInit {
  maxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) { 
    var now = moment();
    this.maxStartTime = moment();
    this.defaultStartTime = moment([now.year(), now.month(), now.date(), 0, 0, 0, 0]);
  }

  ngOnInit() {
  }
}
