import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { Moment } from 'moment'
import moment from 'moment';

@Component({
    selector: 'app-profile-day',
    templateUrl: './profile-day.component.html',
    styleUrls: ['./profile-day.component.css'],
    standalone: false
})
export class ProfileDayComponent implements OnInit {
  dpMaxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) { 
    var now = moment();
    this.dpMaxStartTime = moment().add(1, 'd');
    this.defaultStartTime = moment([now.year(), now.month(), now.date(), 0, 0, 0, 0]);
  }

  ngOnInit() {
  }
}
