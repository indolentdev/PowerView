import { Component, OnInit, Injectable } from '@angular/core';
import { NGXLogger } from 'ngx-logger';

import { DateAdapter } from '@angular/material/core';
import { MomentDateAdapter } from '@angular/material-moment-adapter';

import { Moment } from 'moment'
import moment from 'moment';


@Injectable()
export class MonthCustomMomentDateAdapter extends MomentDateAdapter {
  format(date: Moment, displayFormat: Object): string {
    var formatString = 'MMMM YYYY';
    return date.format(formatString);
  }
}

@Component({
    selector: 'app-profile-month',
    templateUrl: './profile-month.component.html',
    styleUrls: ['./profile-month.component.css'],
    providers: [
        { provide: DateAdapter, useClass: MonthCustomMomentDateAdapter }
    ],
    standalone: false
})
export class ProfileMonthComponent implements OnInit {
  dpMaxStartTime: Moment;
  defaultStartTime: Moment;

  constructor(private log: NGXLogger) { 
    var now = moment();
    this.dpMaxStartTime = moment([now.year(), now.month(), 1, 0, 0, 0, 0]);
    this.defaultStartTime = moment([now.year(), now.month(), 1, 0, 0, 0, 0]);
  }

  ngOnInit() {
  }
}
